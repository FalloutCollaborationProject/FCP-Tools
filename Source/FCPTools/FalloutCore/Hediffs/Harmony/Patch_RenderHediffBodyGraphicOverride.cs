using HarmonyLib;

namespace FCP.Core.Hediffs;

[HarmonyPatch(typeof(PawnRenderNode_AnimalPart), "GraphicFor")]
public static class Patch_RenderHediffBodyGraphicOverride
{
    public static bool Prefix(PawnRenderNode_AnimalPart __instance, Pawn pawn, ref Graphic __result)
    {
        if (__instance is not PawnRenderNode_AnimalPart_Body)
            return true;

        HediffComp_BodyGraphicOverride overrideComp = pawn?.health?.hediffSet?.hediffs
            .OfType<HediffWithComps>()
            .Select(h => h.TryGetComp<HediffComp_BodyGraphicOverride>())
            .FirstOrDefault(c => c != null);

        if (overrideComp == null)
            return true;

        __result = overrideComp.BodyGraphic;
        return false;
    }
}
