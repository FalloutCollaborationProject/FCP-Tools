// Decompiled with JetBrains decompiler
// Type: RangerRick_PowerArmor.JobGiver_Reload_TryGiveJob_Patch
// Assembly: RangerRick_PowerArmor, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2F9A23E8-63EA-4090-93FB-DECA3711786E
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\RimWorld\Mods\FCP-Tools\1.5\Assemblies\RangerRick_PowerArmor.dll

using FCP.Core;
using HarmonyLib;
using RimWorld;
using System;
using System.Linq;
using Verse;
using Verse.AI;

#nullable disable
namespace FCP.Core
{
    [HotSwappable]
    public static class JobGiver_Reload_TryGiveJob_Patch
    {
        public static void Postfix(ref Job __result, Pawn pawn)
        {
            if (__result != null || !pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
                return;
            foreach (Pawn otherPawn in pawn.Map.mapPawns.PawnsInFaction(pawn.Faction).Where(x => x.RaceProps.Humanlike).OrderBy(x => x.Position.DistanceTo(pawn.Position)).ToList())
            {
                foreach (Apparel t in otherPawn.apparel.WornApparel)
                {
                    if (t.GetComp<CompPowerArmor>() != null && CanRefuel(pawn, t))
                    {
                        __result = RefuelJob(pawn, t, otherPawn);
                        return;
                    }
                }
            }
        }

        public static bool CanRefuel(Pawn pawn, Thing t, bool forced = false)
        {
            if (pawn.workSettings == null || pawn.WorkTypeIsDisabled(RR_DefOf.Refuel.workType) || pawn.WorkTagIsDisabled(RR_DefOf.Refuel.workTags) || RR_DefOf.Refuel.Worker.MissingRequiredCapacity(pawn) != null)
                return false;
            CompRefuelable comp1 = t.TryGetComp<CompRefuelable>();
            if (comp1 == null || comp1.IsFull || !forced && !comp1.allowAutoRefuel || (double)comp1.FuelPercentOfMax > 0.0 && !comp1.Props.allowRefuelIfNotEmpty || !forced && !comp1.ShouldAutoRefuelNow || t.IsForbidden(pawn) || !pawn.CanReserve((LocalTargetInfo)t, ignoreOtherReservations: forced) || t is Apparel apparel && apparel.Wearer?.Faction != pawn.Faction)
                return false;
            CompInteractable comp2 = t.TryGetComp<CompInteractable>();
            if (comp2 != null && comp2.Props.cooldownPreventsRefuel && comp2.OnCooldown)
            {
                JobFailReason.Is(comp2.Props.onCooldownString.CapitalizeFirst());
                return false;
            }
            if (FindBestFuel(pawn, t) == null)
            {
                JobFailReason.Is((string)"NoFuelToRefuel".Translate((NamedArgument)t.TryGetComp<CompRefuelable>().Props.fuelFilter.Summary));
                return false;
            }
            if (!t.TryGetComp<CompRefuelable>().Props.atomicFueling || FindAllFuel(pawn, t) != null)
                return true;
            JobFailReason.Is((string)"NoFuelToRefuel".Translate((NamedArgument)t.TryGetComp<CompRefuelable>().Props.fuelFilter.Summary));
            return false;
        }

        public static Job RefuelJob(Pawn pawn, Thing t, Pawn otherPawn)
        {
            Thing bestFuel = FindBestFuel(pawn, t);
            return JobMaker.MakeJob(RR_DefOf.RR_RefuelPowerArmor, (LocalTargetInfo)t, (LocalTargetInfo)bestFuel, (LocalTargetInfo)otherPawn);
        }


        private static Thing FindBestFuel(Pawn pawn, Thing refuelable)
        {
            ThingFilter filter = refuelable.TryGetComp<CompRefuelable>().Props.fuelFilter;
            Predicate<Thing> validator = x => !x.IsForbidden(pawn) && pawn.CanReserve((LocalTargetInfo)x) && filter.Allows(x);
            return GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, filter.BestThingRequest, PathEndMode.ClosestTouch, TraverseParms.For(pawn), validator: validator);
        }


        private static List<Thing> FindAllFuel(Pawn pawn, Thing refuelable)
        {
            int countToFullyRefuel = refuelable.TryGetComp<CompRefuelable>().GetFuelCountToFullyRefuel();
            ThingFilter filter = refuelable.TryGetComp<CompRefuelable>().Props.fuelFilter;
            return RefuelWorkGiverUtility.FindEnoughReservableThings(pawn, refuelable.Position, new IntRange(countToFullyRefuel, countToFullyRefuel), t => filter.Allows(t));
        }
    }
}
