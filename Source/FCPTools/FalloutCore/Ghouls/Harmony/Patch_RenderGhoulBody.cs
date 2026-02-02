using HarmonyLib;

namespace FCP.Core.Ghouls;

// Replace vanilla body graphic with ghoul body graphic - let vanilla handle all rendering
[HarmonyPatch(typeof(PawnRenderNode_Body), "GraphicFor")]
public static class Patch_ReplaceVanillaBodyWithGhoulBody
{
    public static bool Prefix(Pawn pawn, ref Graphic __result)
    {
        var ghoulGene = pawn?.genes?.GetFirstGeneOfType<Gene_GhoulBody>();
        if (ghoulGene == null) 
            return true; // Run original method for non-ghouls
            
        // Return our custom ghoul body graphic - vanilla render node system handles everything else
        __result = ghoulGene.GetBodyOverlay(pawn);
        return false; // Skip original method
    }
}