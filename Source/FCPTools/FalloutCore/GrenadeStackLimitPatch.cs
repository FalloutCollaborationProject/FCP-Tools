using HarmonyLib;
using Verse;

namespace FCP.Core;

[HarmonyPatch(typeof(Thing), nameof(Thing.SplitOff))]
public static class GrenadeStackLimit_Thing_SplitOff_Patch
{
    public static void Postfix(Thing __result)
    {
        if (__result == null || FCPCoreMod.Settings.General.consumableGrenades || __result.stackCount <= 1) return;
        if (Thing_PostMake_Patch.IsConsumableGrenade(__result.def))
        {
            __result.stackCount = 1;
        }
    }
}
