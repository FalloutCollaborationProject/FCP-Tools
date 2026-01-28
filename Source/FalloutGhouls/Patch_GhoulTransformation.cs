using HarmonyLib;
using RimWorld;
using Verse;

namespace FCP_Ghoul
{
    [HarmonyPatch(typeof(Hediff), nameof(Hediff.Tick))]
    public static class Patch_Hediff_Tick
    {
        private static HediffDef transformDef;

        public static void Postfix(Hediff __instance)
        {
            if (__instance?.def != HediffDefOf.ToxicBuildup || !__instance.pawn.IsHashIntervalTick(600)) return;

            var pawn = __instance.pawn;
            if (pawn == null || pawn.IsGhoul()) return;

            if (transformDef == null)
                transformDef = DefDatabase<HediffDef>.GetNamed("FCP_Hediff_GhoulTransformation", false);

            var transform = pawn.health.hediffSet.GetFirstHediffOfDef(transformDef);
            
            if (__instance.Severity >= 0.85f && transform == null)
                pawn.health.AddHediff(transformDef);
            else if (__instance.Severity < 0.7f && transform != null)
                pawn.health.RemoveHediff(transform);
        }
    }
}
