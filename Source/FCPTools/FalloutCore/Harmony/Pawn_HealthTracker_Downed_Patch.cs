using HarmonyLib;
using Verse;
using FCP.Core.Buildings;

namespace FCP.Core.Harmony;

[HarmonyPatch(typeof(Pawn_HealthTracker), nameof(Pawn_HealthTracker.Downed), MethodType.Getter)]
public static class Pawn_HealthTracker_Downed_Patch
{
    [HarmonyPostfix]
    public static void Postfix(Pawn_HealthTracker __instance, ref bool __result)
    {
        if (!__result)
            return;
            
        Pawn pawn = __instance.hediffSet?.pawn;
        if (pawn == null)
            return;
            
        if (pawn.health.hediffSet.HasHediff(Buildings.HediffDefOf.FCP_Crucified))
        {
            __result = false;
        }
    }
}
