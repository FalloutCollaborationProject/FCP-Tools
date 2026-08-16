using HarmonyLib;

namespace FCP.Core.PowerArmor;

[HarmonyPatch(typeof(Pawn), "WorkTypeIsDisabled")]
public static class Pawn_WorkTypeIsDisabled_Patch
{
    public static void Postfix(WorkTypeDef w, Pawn __instance, ref bool __result)
    {
        if (__result) return;
        if (__instance.apparel == null) return;

        List<Apparel> wornApparel = __instance.apparel.WornApparel;

        foreach (Apparel apparel in wornApparel)
        {
            var comp = apparel.TryGetComp<CompPowerArmor>();

            if (comp?.Props.workDisables != null && comp.Props.workDisables.Contains(w))
            {
                __result = true;
                return;
            }
        }
    }
}