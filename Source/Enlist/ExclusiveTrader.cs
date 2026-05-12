using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

namespace FCP.Enlist;

public class ExclusiveTrader : IExposable, ITrader, IThingHolder
{
	private ThingOwner things;
	private int randomPriceFactorSeed = -1;

	public Caravan caravan;
	public Faction faction;
	public FactionEnlistOptionsDef factionOptionDef;

	public TraderKindDef traderKindDef;
	public string traderNameKey;

	public TradeCurrency TradeCurrency => TraderKind.tradeCurrency;
	public IThingHolder ParentHolder => null;
	public TraderKindDef TraderKind => traderKindDef ?? factionOptionDef.exclusiveTraderKind;
	public int RandomPriceFactorSeed => randomPriceFactorSeed;
	public float TradePriceImprovementOffsetForPlayer => 0f;
	public string TraderName => faction.Name + "\n" + (traderNameKey != null ? traderNameKey.Translate() : factionOptionDef.exclusiveTraderLabelKey.Translate());
	public bool CanTradeNow => true;
	public Faction Faction => faction;

	public int Silver
	{
		get
		{
			ThingDef currency = factionOptionDef?.currencyDef ?? ThingDefOf.Silver;
			return CountHeldOf(currency);
		}
	}

	public IEnumerable<Thing> Goods => things;

	public ExclusiveTrader()
	{
		things = new ThingOwner<Thing>(this);
		randomPriceFactorSeed = Rand.RangeInclusive(1, 10000000);
	}

	public IEnumerable<Thing> ColonyThingsWillingToBuy(Pawn playerNegotiator)
	{
		return CaravanInventoryUtility.AllInventoryItems(caravan);
	}

	public void GenerateThings()
	{
		things.ClearAndDestroyContents();
		ThingSetMakerParams parms = default;
		parms.traderDef = TraderKind;
		parms.makingFaction = faction;
		things.TryAddRangeOrTransfer(ThingSetMakerDefOf.TraderStock.root.Generate(parms));
		ThingDef currency = factionOptionDef?.currencyDef ?? ThingDefOf.Silver;
		Thing silver = ThingMaker.MakeThing(currency);
		silver.stackCount = 5000;
		things.TryAdd(silver);
	}

	public void TraderTick() { }

	public void GiveSoldThingToTrader(Thing toGive, int countToGive, Pawn playerNegotiator)
	{
		Thing thing = toGive.SplitOff(countToGive);
		thing.PreTraded(TradeAction.PlayerSells, playerNegotiator, this);
		Thing mergeTarget = TradeUtility.ThingFromStockToMergeWith(this, thing);
		if (mergeTarget != null)
		{
			if (!mergeTarget.TryAbsorbStack(thing, respectStackLimit: false))
				thing.Destroy();
			return;
		}
		things.TryAddOrTransfer(thing, canMergeWithExistingStacks: false);
	}

	public void GiveSoldThingToPlayer(Thing toGive, int countToGive, Pawn playerNegotiator)
	{
		Thing thing = toGive.SplitOff(countToGive);
		thing.PreTraded(TradeAction.PlayerBuys, playerNegotiator, this);
		CaravanInventoryUtility.GiveThing(caravan, thing);
	}

	public int CountHeldOf(ThingDef thingDef, ThingDef stuffDef = null)
	{
		return HeldThingMatching(thingDef, stuffDef)?.stackCount ?? 0;
	}

	public void ChangeCountHeldOf(ThingDef thingDef, ThingDef stuffDef, int count)
	{
		Thing thing = HeldThingMatching(thingDef, stuffDef);
		if (thing != null)
			thing.stackCount += count;
	}

	private Thing HeldThingMatching(ThingDef thingDef, ThingDef stuffDef)
	{
		for (int i = 0; i < things.Count; i++)
			if (things[i].def == thingDef && things[i].Stuff == stuffDef)
				return things[i];
		return null;
	}

	public ThingOwner GetDirectlyHeldThings() => things;

	public void GetChildHolders(List<IThingHolder> outChildren)
	{
		ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
	}

	public void ExposeData()
	{
		Scribe_Deep.Look(ref things, "things", this);
		Scribe_Values.Look(ref randomPriceFactorSeed, "randomPriceFactorSeed");
		Scribe_References.Look(ref faction, "faction");
		Scribe_Defs.Look(ref factionOptionDef, "factionOptionDef");
		Scribe_Defs.Look(ref traderKindDef, "traderKindDef");
		Scribe_Values.Look(ref traderNameKey, "traderNameKey");
	}
}
