using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;

namespace FCP.Core.Ghouls
{
    [HarmonyPatch(typeof(PawnCapacityUtility), nameof(PawnCapacityUtility.CalculateCapacityLevel))]
    public static class Patch_PawnCapacityUtility_CalculateCapacityLevel
    {
        public static void Prefix(HediffSet diffSet, PawnCapacityDef capacity, ref List<PawnCapacityUtility.CapacityImpactor> impactors, bool forTradePrice)
        {
            if (impactors == null || diffSet?.pawn == null)
                return;
                
            if (!diffSet.pawn.health.hediffSet.HasHediff(HediffDefOf_Ghoul.ToxicHealing))
                return;

            impactors.RemoveAll(imp => 
            {
                var hediffField = imp.GetType().GetField("hediff", BindingFlags.Public | BindingFlags.Instance);
                if (hediffField != null)
                {
                    var hediff = hediffField.GetValue(imp) as Hediff;
                    return hediff?.def == HediffDefOf.ToxicBuildup;
                }
                return false;
            });
        }
    }

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
}
