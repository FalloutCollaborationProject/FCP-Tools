using HarmonyLib;

namespace FCP.Core;

[HarmonyPatch(typeof(Pawn_ApparelTracker), nameof(Pawn_ApparelTracker.Wear))]
public static class Pawn_ApparelTracker_Wear_Patch
{
    public static bool Prefix(Pawn_ApparelTracker __instance, Apparel newApparel)
    {
        if (__instance.WouldReplaceLockedApparel(newApparel))
        {
            FCPLog.Warning($"Blocked Wear() of {newApparel} on {__instance.pawn} because it would replace locked apparel");
            return false;
        }

        return true;
    }
}

[HarmonyPatch(typeof(Pawn_ApparelTracker), nameof(Pawn_ApparelTracker.Remove))]
public static class Pawn_ApparelTracker_Remove_Patch
{
    public static bool Prefix(Pawn_ApparelTracker __instance, Apparel ap)
    {
        if (__instance.IsLocked(ap))
        {
            FCPLog.Warning($"Blocked Remove() of locked apparel {ap} from {__instance.pawn}");
            return false;
        }

        return true;
    }
}

[HarmonyPatch]
public static class Pawn_ApparelTracker_TryDrop_Patch
{
    public static System.Reflection.MethodBase TargetMethod()
    {
        return AccessTools.Method(typeof(Pawn_ApparelTracker), nameof(Pawn_ApparelTracker.TryDrop),
            new[] { typeof(Apparel), typeof(Apparel).MakeByRefType(), typeof(IntVec3), typeof(bool) });
    }

    public static bool Prefix(Pawn_ApparelTracker __instance, Apparel ap, ref Apparel resultingAp, ref bool __result)
    {
        if (__instance.IsLocked(ap))
        {
            FCPLog.Warning($"Blocked TryDrop() of locked apparel {ap} from {__instance.pawn}");
            resultingAp = null;
            __result = false;
            return false;
        }

        return true;
    }
}
