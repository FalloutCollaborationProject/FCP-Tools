using HarmonyLib;

namespace FCP.Core.Abolitionists;

[HarmonyPatch(typeof(Pawn_GuestTracker), nameof(Pawn_GuestTracker.Released), MethodType.Setter)]
public static class Pawn_GuestTracker_Released_Patch
{
    private static readonly AccessTools.FieldRef<Pawn_GuestTracker, Pawn> PawnField =
        AccessTools.FieldRefAccess<Pawn_GuestTracker, Pawn>("pawn");

    [HarmonyPrefix]
    public static void Prefix(Pawn_GuestTracker __instance, bool value)
    {
        if (!value || __instance.Released)
        {
            return;
        }

        Pawn pawn = PawnField(__instance);
        if (pawn == null || !pawn.IsSlave)
        {
            return;
        }

        AbolitionistsUtility.RewardSlaveRelease(pawn);
    }
}
