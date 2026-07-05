using HarmonyLib;
using Verse;

namespace FCP.Core;

[HarmonyPatch(typeof(Thing), nameof(Thing.TryAbsorbStack))]
public static class GrenadeStackAbsorb_Patch
{
    public static bool Prefix(Thing __instance, Thing other, ref bool __result)
    {
        if (FCPCoreMod.Settings.General.consumableGrenades) return true;
        if (!Thing_PostMake_Patch.IsConsumableGrenade(__instance.def)) return true;
        __result = false;
        return false;
    }
}
