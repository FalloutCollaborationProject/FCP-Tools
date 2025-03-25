using Verse;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse.AI.Group;

namespace FCP.Core
{
    public static class MercenaryUtility
    {
        public static void HireMercenary(Pawn pawn, string groupName = "Default")
        {
            if (pawn == null || pawn.Faction == null) return;

            var data = pawn.Faction.GetMercenaryData();
            if (data == null) return;
            var group = data.groups.FirstOrDefault(g => g.name == groupName);
            if (group == null)
            {
                group = new MercenaryGroup { name = groupName };
                data.groups.Add(group);
            }
            if (!group.members.Contains(pawn))
            {
                group.members.Add(pawn);
                data.reservedArrivals.RemoveAll(a => a.pawnKind == pawn.kindDef && a.faction == pawn.Faction);
            }
        }

        public static void ReleaseMercenary(Pawn pawn)
        {
            if (pawn == null || pawn.Faction == null) return;

            var data = pawn.Faction.GetMercenaryData();
            if (data == null) return;

            foreach (var group in data.groups)
            {
                group.members.Remove(pawn);
            }
        }

        public static bool IsMercenaryHired(Pawn pawn)
        {
            if (pawn == null || pawn.Faction == null) return false;

            var data = pawn.Faction.GetMercenaryData();
            if (data == null) return false;

            return data.groups.Any(g => g.members.Contains(pawn));
        }

        public static List<Pawn> GetHiredMercenaries(Faction faction, string groupName = null)
        {
            if (faction == null) return new List<Pawn>();

            var data = faction.GetMercenaryData();
            if (data == null) return new List<Pawn>();

            return groupName != null
                ? data.groups.FirstOrDefault(g => g.name == groupName)?.members.ToList() ?? new List<Pawn>()
                : data.AllHiredPawns;
        }

        public static List<Pawn> GetAllHiredMercenaries()
        {
            return Find.FactionManager.AllFactionsListForReading
                .SelectMany(f => f.GetMercenaryData()?.AllHiredPawns ?? Enumerable.Empty<Pawn>())
                .Distinct()
                .ToList();
        }

        public static void ReserveMercenary(Faction faction, PawnKindDef pawnKind, Map destinationMap, int delayTicks)
        {
            if (faction == null || pawnKind == null || destinationMap == null) return;

            var data = faction.GetMercenaryData();
            if (data == null) return;

            var arrival = new QueuedMercenaryArrival
            {
                faction = faction,
                pawnKind = pawnKind,
                arrivalTick = Find.TickManager.TicksGame + delayTicks,
                destinationMap = destinationMap
            };
            data.reservedArrivals.Add(arrival);
        }

        public static bool TrySendMercenaryCaravan(Faction faction, List<Pawn> pawns, CaravanObjectiveDef objective, int durationDays)
        {
            var data = faction.GetMercenaryData(); // Re-add data variable
            if (faction == null || pawns.NullOrEmpty() || objective == null || durationDays <= 0 || data == null)
            {
                Log.Error("FCP: Invalid parameters for sending mercenary caravan.");
                return false;
            }
            // Calculate success chance from objective def
            float successChance = objective.successRate;
            int currentTick = Find.TickManager.TicksGame;
            int returnTick = currentTick + (durationDays * GenDate.TicksPerDay);
            var caravanData = new MercenaryCaravanData(faction, pawns, objective, currentTick, returnTick, successChance);
            data.activeCaravans.Add(caravanData);
            foreach (var p in pawns)
            {
                p.GetLord()?.Notify_PawnLost(p, PawnLostCondition.ForcedToJoinOtherLord);
                if (p.Spawned)
                {
                    p.DeSpawn(DestroyMode.QuestLogic);
                }
            }

            Messages.Message("MessageMercenaryCaravanSent".Translate(pawns.Count, faction.Name, objective.label, durationDays), MessageTypeDefOf.PositiveEvent);
            return true;
        }

        public static bool PlayerHasEnoughSilver(float amount)
        {
            if (amount <= 0) return true;
            int amountNeeded = (int)amount;
            int currentSilver = 0;
            foreach (Map map in Find.Maps.Where(m => m.IsPlayerHome))
            {
                currentSilver += map.resourceCounter.GetCount(ThingDefOf.Silver);
            }
            return currentSilver >= amountNeeded;
        }

        public static bool TryPayCost(float amount)
        {
            if (amount <= 0) return true;

            int amountNeeded = (int)amount;
            int amountPaid = 0;
            List<Thing> paymentItems = new List<Thing>();
            if (!TryPayWithCurrency(ThingDefOf.Silver, amountNeeded, ref amountPaid, paymentItems))
            {
                var faction = Find.FactionManager.AllFactionsVisible
                    .FirstOrDefault(f => f.def.HasModExtension<MercenaryExtension>());

                if (faction != null)
                {
                    var extension = faction.def.GetModExtension<MercenaryExtension>();
                    if (extension?.paymentMethods != null)
                    {
                        foreach (var paymentMethod in extension.paymentMethods)
                        {
                            if (TryPayWithCurrency(paymentMethod, amountNeeded - amountPaid, ref amountPaid, paymentItems))
                                break;
                        }
                    }
                }
            }

            bool success = amountPaid >= amountNeeded;
            if (!success)
            {
                Messages.Message("MessageMercenaryPaymentFailed".Translate(amountNeeded.ToString("F0")), MessageTypeDefOf.NegativeEvent);
                Log.Warning($"FCP: Payment failed - needed {amountNeeded}, could only pay {amountPaid}");
            }
            else
            {
                Messages.Message("MessageMercenaryPaymentSuccess".Translate(amountNeeded.ToString("F0")), MessageTypeDefOf.PositiveEvent);
            }
            paymentItems.ForEach(t => t.Destroy());
            return success;
        }

        private static bool TryPayWithCurrency(ThingDef currencyDef, int amountNeeded, ref int amountPaid, List<Thing> paymentItems)
        {
            foreach (Map map in Find.Maps.Where(m => m.IsPlayerHome))
            {
                foreach (Thing currency in map.listerThings.ThingsOfDef(currencyDef))
                {
                    if (!currency.IsForbidden(Faction.OfPlayer))
                    {
                        int canTake = Math.Min(amountNeeded - amountPaid, currency.stackCount);
                        if (canTake > 0)
                        {
                            Thing taken = currency.SplitOff(canTake);
                            paymentItems.Add(taken);
                            amountPaid += canTake;
                            if (amountPaid >= amountNeeded)
                                return true;
                        }
                    }
                }
            }
            return false;
        }

        public static bool PlayerHasEnoughTribute(ThingDef itemDef, int amount)
        {
            if (itemDef == null || amount <= 0) return true;

            int currentAmount = 0;
            foreach (Map map in Find.Maps.Where(m => m.IsPlayerHome))
            {
                currentAmount += map.resourceCounter.GetCount(itemDef);
            }
            return currentAmount >= amount;
        }

        public static bool TryRemoveTributeItems(ThingDef itemDef, int amount)
        {
            if (itemDef == null || amount <= 0) return true;

            int amountToRemove = amount;
            int removedAmount = 0;
            List<Thing> itemsToDestroy = new List<Thing>();

            foreach (Map map in Find.Maps.Where(m => m.IsPlayerHome))
            {
                List<Thing> itemStacks = map.listerThings.ThingsOfDef(itemDef);
                foreach (Thing item in itemStacks)
                {
                    if (!item.IsForbidden(Faction.OfPlayer))
                    {
                        int canTake = System.Math.Min(amountToRemove - removedAmount, item.stackCount);
                        if (canTake > 0)
                        {
                            Thing takenItem = item.SplitOff(canTake);
                            itemsToDestroy.Add(takenItem);
                            removedAmount += canTake;
                            if (removedAmount >= amountToRemove)
                            {
                                itemsToDestroy.ForEach(i => i.Destroy(DestroyMode.Vanish));
                                return true;
                            }
                        }
                    }
                }
            }
            if (removedAmount < amountToRemove)
            {
                Log.Warning($"FCP: Failed to remove tribute items. Needed {amountToRemove} of {itemDef.label}, found and could take {removedAmount}.");
            }
            itemsToDestroy.ForEach(i => i.Destroy(DestroyMode.Vanish));
            return removedAmount >= amountToRemove;
        }
    }
}
