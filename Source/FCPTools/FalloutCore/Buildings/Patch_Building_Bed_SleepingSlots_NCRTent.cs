using HarmonyLib;
using RimWorld;

namespace FCP.Core.Buildings;

[HarmonyPatch(typeof(Building_Bed), nameof(Building_Bed.SleepingSlotsCount), MethodType.Getter)]
public static class Patch_Building_Bed_SleepingSlots_NCRTent
{
    private const int TentSleepingSlots = 3;

    public static void Postfix(Building_Bed __instance, ref int __result)
    {
        if (__instance.def == FCPDefOf.FCP_Building_NCR_Tent)
            __result = TentSleepingSlots;
    }
}
