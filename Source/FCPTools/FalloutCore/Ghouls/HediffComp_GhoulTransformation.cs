using RimWorld;
using Verse;
using UnityEngine;

namespace FCP.Core.Ghouls
{
    public class HediffComp_GhoulTransformation : HediffComp
    {
        private bool hasRolled;
        private const float TOXIC_THRESHOLD = 0.90f;

        public HediffCompProperties_GhoulTransformation Props => (HediffCompProperties_GhoulTransformation)props;

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);

            if (!Pawn.IsHashIntervalTick(600)) return;

            var toxic = Pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.ToxicBuildup);
            if (toxic == null || toxic.Severity < TOXIC_THRESHOLD)
            {
                hasRolled = false;
                return;
            }

            if (!hasRolled)
            {
                hasRolled = true;
                ProcessTransformation();
            }
        }

        private void ProcessTransformation()
        {
            if (Pawn.Dead || Pawn.IsGhoul()) return;

            float roll = Rand.Value;
            
            if (roll < 0.66f)
            {
                Pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.ToxicBuildup).Severity = 1f;
                return;
            }
            
            if (roll < 0.86f) ApplyTransform("FCP_Xenotype_Ghoul_Feral", false, false, true);
            else if (roll < 0.93f) ApplyTransform("FCP_Xenotype_Ghoul", true, true, false);
            else if (roll < 0.98f) ApplyTransform("FCP_Xenotype_Ghoul", false, false, false);
            else if (roll < 0.99f) ApplyTransform("FCP_Xenotype_Ghoul_GlowingOne", true, true, false);
            else ApplyTransform("FCP_Xenotype_Ghoul_GlowingOne", false, false, false);
        }

        private void ApplyTransform(string xenotypeDef, bool addFeral, bool randomFeral, bool maxFeral)
        {
            var xenotype = DefDatabase<XenotypeDef>.GetNamed(xenotypeDef, false);
            if (xenotype == null || Pawn.genes == null) return;

            Pawn.genes.SetXenotype(xenotype);
            
            if (addFeral)
            {
                var feralDef = DefDatabase<GeneDef>.GetNamed("FCP_Gene_Ferality", false);
                if (feralDef != null) Pawn.genes.AddGene(feralDef, false);
            }
            
            if (maxFeral || randomFeral)
            {
                var gene = Pawn.genes.GetFirstGeneOfType<Gene_Ferality>();
                if (gene != null)
                {
                    float val = maxFeral ? 100f : Mathf.Lerp(50f, 95f, Mathf.Pow(Rand.Range(0.5f, 0.95f), 2f));
                    gene.SetFerality(val);
                }
            }
            
            var toxic = Pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.ToxicBuildup);
            if (toxic != null) toxic.Severity = 0.1f;
            
            Pawn.Drawer.renderer.SetAllGraphicsDirty();
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref hasRolled, "hasRolled", false);
        }
    }

    public static class GhoulExtensions
    {
        public static bool IsGhoul(this Pawn pawn)
        {
            if (pawn?.genes?.Xenotype == null) return false;

            string name = pawn.genes.Xenotype.defName;
            return name == "FCP_Xenotype_Ghoul" || name == "FCP_Xenotype_Ghoul_Feral" || 
                   name == "FCP_Xenotype_Ghoul_GlowingOne" || name == "FCP_Xenotype_Ghoul_GlowingOne_Feral";
        }
    }
}
