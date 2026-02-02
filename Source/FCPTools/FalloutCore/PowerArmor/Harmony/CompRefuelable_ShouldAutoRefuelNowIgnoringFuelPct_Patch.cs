using HarmonyLib;

namespace FCP.Core.PowerArmor;

[HarmonyPatch(typeof(CompRefuelable), "ShouldAutoRefuelNowIgnoringFuelPct", MethodType.Getter)]
public static class CompRefuelable_ShouldAutoRefuelNowIgnoringFuelPct_Patch
{
    public static bool Prefix(CompRefuelable __instance, ref bool __result)
    {
        if (__instance.parent.Spawned is false && __instance.parent.GetComp<CompPowerArmor>() is CompPowerArmor comp)
        {
            __result = __instance.parent.IsBurning() is false;
            return false;
        }
        return true;
    }
}