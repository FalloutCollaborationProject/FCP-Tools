using HarmonyLib;

namespace FCP.Core.Ghouls;

[HarmonyPatch(typeof(Thing), nameof(Thing.Ingested))]
public static class Patch_Thing_Ingested
{
    public static void Postfix(Thing __instance, Pawn ingester)
    {
        if (ingester?.genes == null || __instance == null)
            return;

        var feralityGene = ingester.genes.GetFirstGeneOfType<Gene_Ferality>();
        if (feralityGene == null)
            return;

        var ext = feralityGene.def.GetModExtension<FeralityGene_ModExtension>();
        if (ext?.drugs != null && ext.drugs.Contains(__instance.def))
            feralityGene.ReduceFerality(ext.amountReducedPerDrug);
    }
}