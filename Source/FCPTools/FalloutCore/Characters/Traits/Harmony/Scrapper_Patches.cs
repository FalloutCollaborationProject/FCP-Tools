using System;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace FCP.Core.Traits;

internal static class ScrapperContext
{
    public static Pawn CurrentDeconstructor;
}

[HarmonyPatch(typeof(JobDriver_Deconstruct), "FinishedRemoving")]
public static class JobDriverDeconstruct_FinishedRemoving_Patch
{
    public static void Prefix(JobDriver_Deconstruct __instance)
    {
        ScrapperContext.CurrentDeconstructor = __instance.pawn;
    }

    public static void Postfix()
    {
        ScrapperContext.CurrentDeconstructor = null;
    }
}

[HarmonyPatch(typeof(GenLeaving), "GetBuildingResourcesLeaveCalculator")]
public static class GenLeaving_GetBuildingResourcesLeaveCalculator_Patch
{
    private const float ScrapperYieldFactor = 1.5f;

    public static void Postfix(ref Func<int, int> __result)
    {
        Pawn pawn = ScrapperContext.CurrentDeconstructor;
        if (pawn?.story?.traits == null || !pawn.story.traits.HasTrait(TraitsDefOf.FCP_Trait_Scrapper))
            return;

        Func<int, int> baseCalculator = __result;
        __result = costCount => Mathf.CeilToInt(baseCalculator(costCount) * ScrapperYieldFactor);
    }
}
