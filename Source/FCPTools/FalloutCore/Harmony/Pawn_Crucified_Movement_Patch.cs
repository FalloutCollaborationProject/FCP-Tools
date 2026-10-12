using HarmonyLib;
using Verse;
using Verse.AI;
using FCP.Core.Buildings;

namespace FCP.Core.Harmony;

[HarmonyPatch(typeof(Pawn_PathFollower), nameof(Pawn_PathFollower.StartPath))]
public static class Pawn_PathFollower_StartPath_Patch
{
    [HarmonyPrefix]
    public static bool Prefix(Pawn ___pawn)
    {
        if (___pawn?.health?.hediffSet == null)
            return true;

        if (___pawn.health.hediffSet.HasHediff(Buildings.HediffDefOf.FCP_Crucified))
            return false;

        return true;
    }
}

[HarmonyPatch(typeof(Pawn_JobTracker), nameof(Pawn_JobTracker.StartJob))]
public static class Pawn_JobTracker_StartJob_Patch
{
    [HarmonyPrefix]
    public static bool Prefix(Pawn ___pawn)
    {
        if (___pawn?.health?.hediffSet == null)
            return true;

        if (___pawn.health.hediffSet.HasHediff(Buildings.HediffDefOf.FCP_Crucified))
            return false;

        return true;
    }
}
