using HarmonyLib;
using RimWorld;
using UnityEngine;
// ReSharper disable InconsistentNaming

namespace FCP.Core;

[HarmonyPatch(typeof(GenDate), nameof(GenDate.Year), typeof(long), typeof(float))]
public static class Patch_GenDate_Year_StartingYear
{
    public static bool Prefix(long absTicks, float longitude, ref int __result)
    {
        int startingYear = FCPCoreMod.SettingsTab<ScenarioSettings>()?.startingYear ?? GenDate.DefaultStartingYear;
        long num = absTicks + GenDate.LocalTicksOffsetFromLongitude(longitude);
        __result = startingYear + Mathf.FloorToInt((float)num / GenDate.TicksPerYear);
        return false;
    }
}
