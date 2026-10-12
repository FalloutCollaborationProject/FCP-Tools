using HarmonyLib;
using RimWorld.Utility;
using Verse.AI;

namespace FCP.Core;

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
            if (pawn.Faction == null)
            {
                return;
            }
            int bestDist = int.MaxValue;
            List<Pawn> factionPawns = pawn.Map.mapPawns.PawnsInFaction(pawn.Faction);
            for (int i = 0; i < factionPawns.Count; i++)
            {
                Pawn otherPawn = factionPawns[i];
                if (otherPawn == pawn || !otherPawn.RaceProps.Humanlike)
                {
                    continue;
                }
                int dist = otherPawn.Position.DistanceToSquared(pawn.Position);
                if (dist >= bestDist)
                {
                    continue;
                }
                var verb = otherPawn.equipment?.PrimaryEq?.PrimaryVerb as Verb_Shoot_ApparelAmmo;
                if (verb == null)
                {
                    continue;
                }
                var comp = otherPawn.GetAvailableApparelAmmo(verb, out _, out _);
                if (comp == null || comp.RemainingCharges > 0)
                {
                    continue;
                }
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
                bestDist = dist;
                __result = MakeReloadJob(comp, list, otherPawn);
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
