using HarmonyLib;
using UnityEngine;
using Verse;

namespace FCP_Ghoul
{
    // Replace vanilla body graphic with ghoul body graphic - let vanilla handle all rendering
    [HarmonyPatch(typeof(PawnRenderNode_Body), "GraphicFor")]
    public static class Patch_ReplaceVanillaBodyWithGhoulBody
    {
        public static bool Prefix(Pawn pawn, ref Graphic __result)
        {
            if (pawn?.genes == null) return true;

            // Check if pawn has ghoul body gene
            Gene_GhoulBody ghoulGene = pawn.genes.GetFirstGeneOfType<Gene_GhoulBody>();
            if (ghoulGene != null)
            {
                // Return our custom ghoul body graphic - vanilla render node system handles everything else
                __result = ghoulGene.GetBodyOverlay(pawn);
                return false; // Skip original method
            }

            return true; // Run original method for non-ghouls
        }
    }
}
