using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Grammar;

namespace  FCP.Enlist
{
    public class PawnTrader : IExposable, ITrader, IThingHolder
    {
        private ThingOwner things;

        private List<Pawn> soldPrisoners = new List<Pawn>();

        public Caravan caravan;

        private int randomPriceFactorSeed = -1;
        public int Silver => CountHeldOf(ThingDefOf.Silver);
        public TradeCurrency TradeCurrency => TraderKind.tradeCurrency;
        public IThingHolder ParentHolder => null;

        public FactionEnlistOptionsDef factionOptionDef;
        public TraderKindDef TraderKind => factionOptionDef.turnInTraderKind;
        public int RandomPriceFactorSeed => randomPriceFactorSeed;
        public float TradePriceImprovementOffsetForPlayer => 0f;
        public IEnumerable<Thing> Goods
        {
            get
            {

                for (int i = 0; i < things.Count; i++)
                {
                    if (things[i].def == ThingDefOf.Silver)
                    {
                        yield return things[i];
                    }
                }
            }
        }
        public string TraderName => Faction.Name + "\n" + factionOptionDef.turnInTraderNameKey.Translate();
        public bool CanTradeNow => true;

        public Faction faction;
        public Faction Faction => faction;

        public PawnTrader()
        {
            things = new ThingOwner<Thing>(this);
            randomPriceFactorSeed = Rand.RangeInclusive(1, 10000000);
        }

        public IEnumerable<Thing> ColonyThingsWillingToBuy(Pawn playerNegotiator)
        {
            foreach (Pawn item2 in AllSellableColonyPawns(playerNegotiator))
            {
                item2.guest.joinStatus = JoinStatus.JoinAsColonist;
                yield return item2;
            }
        }

        public IEnumerable<Pawn> AllSellableColonyPawns(Pawn negotiator)
        {
            foreach (Pawn item in negotiator.GetCaravan().PawnsListForReading.Where(x => x.IsPrisoner || x.IsSlave))
            {
                item.guest.joinStatus = JoinStatus.JoinAsColonist;
                yield return item;
            }
        }

        public void GenerateThings()
        {
            things.ClearAndDestroyContents();
            ThingSetMakerParams parms = default(ThingSetMakerParams);
            parms.traderDef = TraderKind;
            var generatedThings = ThingSetMakerDefOf.TraderStock.root.Generate(parms);
            things.TryAddRangeOrTransfer(generatedThings);
        }

        public void TraderTick()
        {
            for (int num = things.Count - 1; num >= 0; num--)
            {
                Pawn pawn = things[num] as Pawn;
                if (pawn != null)
                {
                    pawn.Tick();
                    if (pawn.Dead)
                    {
                        things.Remove(pawn);
                    }
                }
            }
        }

        public void ExposeData()
        {
            Scribe_Deep.Look(ref things, "things", this);
            Scribe_Collections.Look(ref soldPrisoners, "soldPrisoners", LookMode.Reference);
            Scribe_Values.Look(ref randomPriceFactorSeed, "randomPriceFactorSeed", 0);
            Scribe_References.Look(ref faction, "faction");
            Scribe_Defs.Look(ref factionOptionDef, "factionOptionDef");
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                soldPrisoners.RemoveAll((Pawn x) => x == null);
            }
        }
        public int CountHeldOf(ThingDef thingDef, ThingDef stuffDef = null)
        {
            return HeldThingMatching(thingDef, stuffDef)?.stackCount ?? 0;
        }

        public void GiveSoldThingToTrader(Thing toGive, int countToGive, Pawn playerNegotiator)
        {
            Thing thing = toGive.SplitOff(countToGive);
            thing.PreTraded(TradeAction.None, playerNegotiator, this);
            Thing thing2 = TradeUtility.ThingFromStockToMergeWith(this, thing);
            if (thing2 != null)
            {
                if (!thing2.TryAbsorbStack(thing, respectStackLimit: false))
                {
                    thing.Destroy();
                }
                return;
            }
            Pawn pawn = thing as Pawn;
            if (pawn != null && pawn.RaceProps.Humanlike)
            {
                var caravan = playerNegotiator.GetCaravan();
                CaravanInventoryUtility.MoveAllInventoryToSomeoneElse(pawn, caravan.PawnsListForReading.Where(x => x.IsColonist).Except(pawn).ToList());
                foreach (Pawn otherPawn in PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_FreeColonistsAndPrisoners)
                {
                    if (factionOptionDef.turnInThought != null && pawn != otherPawn && otherPawn.IsColonist && otherPawn.needs.mood != null)
                    {
                        otherPawn.needs.mood.thoughts.memories.TryGainMemory(factionOptionDef.turnInThought);
                    }
                }
            }
            things.TryAddOrTransfer(thing, canMergeWithExistingStacks: false);
            if (pawn != null && pawn.IsWorldPawn())
            {
                Find.WorldPawns.RemovePawn(pawn);
            }
        }

        public void GiveSoldThingToPlayer(Thing toGive, int countToGive, Pawn playerNegotiator)
        {
            caravan.trader.GiveSoldThingToPlayer(toGive, countToGive, playerNegotiator);
        }

        private Thing HeldThingMatching(ThingDef thingDef, ThingDef stuffDef)
        {
            for (int i = 0; i < things.Count; i++)
            {
                if (things[i].def == thingDef && things[i].Stuff == stuffDef)
                {
                    return things[i];
                }
            }
            return null;
        }

        public void ChangeCountHeldOf(ThingDef thingDef, ThingDef stuffDef, int count)
        {
            Thing thing = HeldThingMatching(thingDef, stuffDef);
            if (thing == null)
            {
                Log.Error("Changing count of thing trader doesn't have: " + thingDef);
            }
            thing.stackCount += count;
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return things;
        }

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
        }
    }
}