using HarmonyLib;
using Verse;
using Verse.AI;
using FCP.Core.Buildings;

namespace FCP.Core.Harmony;

[HarmonyPatch(typeof(Pawn_PathFollower), nameof(Pawn_PathFollower.StartPath))]
public static class Pawn_PathFollower_StartPath_Patch
{
    [HarmonyPrefix]
    public static bool Prefix(Pawn_PathFollower __instance)
    {
        Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
        if (pawn?.health?.hediffSet == null)
            return true;
            
        if (pawn.health.hediffSet.HasHediff(Buildings.HediffDefOf.FCP_Crucified))
            return false;
            
        return true;
    }
}

[HarmonyPatch(typeof(Pawn_JobTracker), nameof(Pawn_JobTracker.StartJob))]
public static class Pawn_JobTracker_StartJob_Patch
{
    [HarmonyPrefix]
    public static bool Prefix(Pawn_JobTracker __instance)
    {
        Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
        if (pawn?.health?.hediffSet == null)
            return true;
            
        if (pawn.health.hediffSet.HasHediff(Buildings.HediffDefOf.FCP_Crucified))
            return false;
            
        return true;
    }
}
