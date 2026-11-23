using FCP.Core.Hediffs;
using HarmonyLib;

namespace FCP.Core.Synths;

[HarmonyPatch(typeof(PawnRenderNode_Body), "GraphicFor")]
public static class Patch_RenderSynthBody
{
    public static bool Prefix(Pawn pawn, ref Graphic __result)
    {
        var synthGene = pawn?.genes?.GetFirstGeneOfType<Gene_SynthBody>();
        if (synthGene == null || pawn.health.hediffSet.hediffs.Any(h => h.TryGetComp<HediffComp_BodyGraphicOverride>() != null))
            return true;

        __result = synthGene.GetBodyOverlay(pawn);
        return false;
    }
}
