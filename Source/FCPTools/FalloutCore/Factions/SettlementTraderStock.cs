using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace FCP.Factions;

public class SettlementTraderStock : ITrader, IThingHolder
{
	private Settlement settlement;
	private TraderKindDef kind;
	private ThingOwner things;
	private int seed;

	public SettlementTraderStock(Settlement settlement, TraderKindDef traderKind)
	{
		this.settlement = settlement;
		kind = traderKind;
		things = new ThingOwner<Thing>(this, false);
		seed = Find.TickManager.TicksAbs + settlement.ID * 3541;
		GenerateStock();
	}

	public string TraderName => settlement.Label;
	public bool CanTradeNow => !settlement.Faction.HostileTo(Faction.OfPlayer);
	public float TradePriceImprovementOffsetForPlayer => 0f;
	public TraderKindDef TraderKind => kind;
	public int RandomSeed => seed;
	public int RandomPriceFactorSeed => seed;
	public TradeCurrency TradeCurrency => kind.tradeCurrency;
	public IThingHolder ParentHolder => settlement;
	public Faction Faction => settlement.Faction;
	public IEnumerable<Thing> Goods => things;
	public int Silver
	{
		get
		{
			int num = 0;
			for (int i = 0; i < things.Count; i++)
			{
				if (things[i].def == ThingDefOf.Silver)
					num += things[i].stackCount;
			}
			return num;
		}
	}

	public IEnumerable<Thing> ColonyThingsWillingToBuy(Pawn playerNegotiator)
	{
		Caravan caravan = playerNegotiator.GetCaravan();
		for (int i = 0; i < caravan.pawns.Count; i++)
		{
			Pawn pawn = caravan.pawns[i];
			if (!pawn.inventory.UnloadEverything)
			{
				for (int j = 0; j < pawn.inventory.innerContainer.Count; j++)
					yield return pawn.inventory.innerContainer[j];
			}
		}
	}

	public void GiveSoldThingToTrader(Thing toGive, int countToGive, Pawn playerNegotiator)
	{
		Thing thing = toGive.SplitOff(countToGive);
		thing.PreTraded(TradeAction.PlayerSells, playerNegotiator, this);
		Thing thing2 = TradeUtility.ThingFromStockToMergeWith(this, thing);
		if (thing2 != null)
		{
			if (!thing2.TryAbsorbStack(thing, false))
				thing.Destroy();
		}
		else
			things.TryAdd(thing, false);
	}

	public void GiveSoldThingToPlayer(Thing toGive, int countToGive, Pawn playerNegotiator)
	{
		Caravan caravan = playerNegotiator.GetCaravan();
		Thing thing = toGive.SplitOff(countToGive);
		thing.PreTraded(TradeAction.PlayerBuys, playerNegotiator, this);
		Pawn pawn = CaravanInventoryUtility.FindPawnToMoveInventoryTo(thing, caravan.PawnsListForReading, null);
		if (pawn != null)
			pawn.inventory.innerContainer.TryAdd(thing);
		else
			thing.Destroy();
	}

	public ThingOwner GetDirectlyHeldThings() => things;
	public void GetChildHolders(List<IThingHolder> outChildren) {}

	void GenerateStock()
	{
		things.Clear();
		ThingSetMakerParams parms = default(ThingSetMakerParams);
		parms.traderDef = kind;
		parms.tile = settlement.Tile;
		parms.makingFaction = settlement.Faction;
		things.TryAddRangeOrTransfer(ThingSetMakerDefOf.TraderStock.root.Generate(parms));
	}
}
