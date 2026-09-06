using HarmonyLib;

namespace FCP.Core.Hediffs;

[HarmonyPatch(typeof(PawnRenderNode_Body), "GraphicFor")]
public static class Patch_RenderHediffBodyGraphicOverride_Humanlike
{
    public static bool Prefix(Pawn pawn, ref Graphic __result)
    {
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
