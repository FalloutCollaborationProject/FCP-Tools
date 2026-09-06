using HarmonyLib;

namespace FCP.Core;

[HarmonyPatch(typeof(Pawn), "GetGizmos")]
public static class Pawn_GetGizmos_Syringer_Patch
{
    [HarmonyPostfix]
    public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result, Pawn __instance)
    {
        foreach (Gizmo gizmo in __result)
            yield return gizmo;

        Comp_SyringerAmmo comp = __instance.equipment?.Primary?.TryGetComp<Comp_SyringerAmmo>();
        if (comp == null)
            yield break;

        foreach (Gizmo gizmo in comp.CompGetGizmosExtra())
            yield return gizmo;
    }
}
