using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace FalloutCore
{
    [HarmonyPatch(typeof(HealthUtility), "AdjustSeverity")]
    internal static class AdjustSeverity_Patch
    {
        public static bool Prefix(Pawn pawn, HediffDef hdDef, float sevOffset)
        {
            if (hdDef == HediffDefOf.ToxicBuildup)
            {
                if (pawn.IsGhoul())
                {
                    PawnUtils.TryHeal(pawn);
                    return false;
                }
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Pawn_HealthTracker), "AddHediff", new Type[]
    {
        typeof(Hediff), typeof(BodyPartRecord), typeof(DamageInfo?), typeof(DamageWorker.DamageResult)
    })]
    public static class AddHediff_Patch
    {
        private static bool Prefix(Pawn_HealthTracker __instance, Pawn ___pawn, Hediff hediff, BodyPartRecord part = null, DamageInfo? dinfo = null, DamageWorker.DamageResult result = null)
        {
            if (hediff.def == HediffDefOf.ToxicBuildup)
            {
                if (___pawn.IsGhoul())
                {
                    PawnUtils.TryHeal(___pawn);
                    return false;
                }
            }
            return true;
        }
    }
}