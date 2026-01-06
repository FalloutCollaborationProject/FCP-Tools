using HarmonyLib;
using Verse.AI;

namespace RangerRick_PowerArmor
{
    [HotSwappable]
    [HarmonyPatch(typeof(JobGiver_Reload), "TryGiveJob")]
    public static class JobGiver_Reload_TryGiveJob_Patch
    {
        public static void Postfix(ref Job __result, Pawn pawn)
        {
            if (__result is null)
            {
                if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
                {
                    return;
                }
                var pawns = pawn.Map.mapPawns.PawnsInFaction(pawn.Faction)
                    .Where(x => x.RaceProps.Humanlike).OrderBy(x => x.Position.DistanceTo(pawn.Position)).ToList();
                foreach (var otherPawn in pawns)
                {
                    foreach (var apparel in otherPawn.apparel.WornApparel)
                    {
                        var powerArmor = apparel.GetComp<CompPowerArmor>();
                        if (powerArmor != null)
                        {
                            if (CanRefuel(pawn, apparel))
                            {
                                __result = RefuelJob(pawn, apparel, otherPawn);
                                return;
                            }
                        }
                    }
                }
            }
        }

        public static bool CanRefuel(Pawn pawn, Thing t, bool forced = false)
        {
            if (pawn.workSettings is null)
            {
                return false;
            }
            if (pawn.WorkTypeIsDisabled(RR_DefOf.Refuel.workType) || pawn.WorkTagIsDisabled(RR_DefOf.Refuel.workTags))
            {
                return false;
            }
            if (RR_DefOf.Refuel.Worker.MissingRequiredCapacity(pawn) != null)
            {
                return false;
            }
            CompRefuelable compRefuelable = t.TryGetComp<CompRefuelable>();
            if (compRefuelable == null || compRefuelable.IsFull || (!forced && !compRefuelable.allowAutoRefuel))
            {
                return false;
            }
            if (compRefuelable.FuelPercentOfMax > 0f && !compRefuelable.Props.allowRefuelIfNotEmpty)
            {
                return false;
            }
            if (!forced && !compRefuelable.ShouldAutoRefuelNow)
            {
                return false;
            }
            if (t.IsForbidden(pawn) || !pawn.CanReserve(t, 1, -1, null, forced))
            {
                return false;
            }
            if (t is Apparel apparel && apparel.Wearer?.Faction != pawn.Faction)
            {
                return false;
            }
            CompInteractable compActivable = t.TryGetComp<CompInteractable>();
            if (compActivable != null && compActivable.Props.cooldownPreventsRefuel && compActivable.OnCooldown)
            {
                JobFailReason.Is(compActivable.Props.onCooldownString.CapitalizeFirst());
                return false;
            }
            if (RefuelWorkGiverUtility.FindBestFuel(pawn, t) == null)
            {
                ThingFilter fuelFilter = t.TryGetComp<CompRefuelable>().Props.fuelFilter;
                JobFailReason.Is("NoFuelToRefuel".Translate(fuelFilter.Summary));
                return false;
            }
            if (t.TryGetComp<CompRefuelable>().Props.atomicFueling && RefuelWorkGiverUtility.FindAllFuel(pawn, t) == null)
            {
                ThingFilter fuelFilter2 = t.TryGetComp<CompRefuelable>().Props.fuelFilter;
                JobFailReason.Is("NoFuelToRefuel".Translate(fuelFilter2.Summary));
                return false;
            }
            return true;
        }

        public static Job RefuelJob(Pawn pawn, Thing t, Pawn otherPawn)
        {
            Thing thing = RefuelWorkGiverUtility.FindBestFuel(pawn, t);
            return JobMaker.MakeJob(RR_DefOf.RR_RefuelPowerArmor, t, thing, otherPawn);
        }
    }
}
