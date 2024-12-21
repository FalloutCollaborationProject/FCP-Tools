using FCP.Core.VATS;
using HarmonyLib;

namespace FCP.Core.Vats;

[HarmonyPatch(typeof(Verb))]
public static class Verb_Patch
{
    [HarmonyPatch(nameof(Verb.OutOfRange))]
    [HarmonyPrefix]
    public static bool OutOfRange_Patch(Verb __instance, ref bool __result, IntVec3 root, LocalTargetInfo targ, CellRect occupiedRect)
    {
        Verb_AbilityVATS vats = __instance as Verb_AbilityVATS;

        if (vats == null)
            return true;

        float minRange = vats.PrimaryWeaponVerbProps.EffectiveMinRange(targ, vats.caster);
        float distance = occupiedRect.ClosestDistSquaredTo(root);
        __result = distance > vats.EffectiveRange * (double)vats.EffectiveRange || distance < minRange * (double)minRange;

        return false;
    }
}
