using HarmonyLib;

namespace FCP.Core.Ghouls;

[HarmonyPatch(typeof(PawnRenderNode_Body), "GraphicFor")]
public static class Patch_ReplaceVanillaBodyWithGhoulBody
{
    public static bool Prefix(Pawn pawn, ref Graphic __result)
    {
        var ghoulGene = pawn?.genes?.GetFirstGeneOfType<Gene_GhoulBody>();
        if (ghoulGene == null)
            return true;

        __result = ghoulGene.GetBodyOverlay(pawn);
        return false;
    }
}