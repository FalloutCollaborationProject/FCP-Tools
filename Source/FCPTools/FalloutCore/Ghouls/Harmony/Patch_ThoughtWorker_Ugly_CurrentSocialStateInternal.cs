using HarmonyLib;

namespace FCP.Core.Ghouls;

[HarmonyPatch(typeof(ThoughtWorker_Ugly), "CurrentSocialStateInternal")]
public static class Patch_ThoughtWorker_Ugly_CurrentSocialStateInternal
{
    public static void Postfix(Pawn pawn, Pawn other, ref ThoughtState __result)
    {
        if (__result.Active && GhoulUtility.IsGhoul(pawn) && GhoulUtility.IsGhoul(other))
            __result = false;
    }
}
