using HarmonyLib;
using RimWorld;
using RimWorld.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace BM_HeavyWeapon
{
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
                    .Where(x => x.RaceProps.Humanlike && x != pawn).OrderBy(x => x.Position.DistanceTo(pawn.Position)).ToList();
                foreach (var otherPawn in pawns)
                {
                    var verb = otherPawn.equipment?.PrimaryEq?.PrimaryVerb as Verb_Shoot_ApparelAmmo;
                    if (verb != null)
                    {
                        var comp = otherPawn.GetAvailableApparelAmmo(verb, out _, out _);
                        if (comp != null && comp.RemainingCharges <= 0)
                        {
                            if (pawn.CanReserve(comp.parent) is false)
                            {
                                continue;
                            }
                            if (pawn.carryTracker.AvailableStackSpace(comp.AmmoDef) < comp.MinAmmoNeeded(allowForcedReload: true))
                            {
                                continue;
                            }
                            List<Thing> list = ReloadableUtility.FindEnoughAmmo(pawn, pawn.Position, comp, forceReload: false);
                            if (list.NullOrEmpty())
                            {
                                continue;
                            }
                            __result = MakeReloadJob(comp, list, otherPawn);
                            return;
                        }
                    }
                }
            }
        }

        public static Job MakeReloadJob(CompApparelReloadable comp, List<Thing> chosenAmmo, Pawn targetPawn)
        {
            Job job = JobMaker.MakeJob(BM_DefOf.BM_ReloadOtherPawns, comp.parent);
            job.targetQueueB = chosenAmmo.Select((Thing t) => new LocalTargetInfo(t)).ToList();
            job.targetC = targetPawn;
            job.count = chosenAmmo.Sum((Thing t) => t.stackCount);
            job.count = Math.Min(job.count, comp.MaxAmmoNeeded(allowForcedReload: true));
            return job;
        }
    }
}
