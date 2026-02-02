using HarmonyLib;

namespace FCP.Core.PowerArmor;

[HarmonyPatch(typeof(Pawn), "WorkTypeIsDisabled")]
public static class Pawn_WorkTypeIsDisabled_Patch
{
    public static void Postfix(WorkTypeDef w, Pawn __instance, ref bool __result)
    {
        if (__result == false)
        {
            var apparel = __instance.apparel?.WornApparel.FirstOrDefault(a => a.TryGetComp<CompPowerArmor>() != null);
            if (apparel != null)
            {
                var compPowerArmor = apparel.GetComp<CompPowerArmor>();
                if (compPowerArmor.Props.workDisables != null && compPowerArmor.Props.workDisables.Contains(w))
                {
                    __result = true;
                }
            }
        }
    }
}