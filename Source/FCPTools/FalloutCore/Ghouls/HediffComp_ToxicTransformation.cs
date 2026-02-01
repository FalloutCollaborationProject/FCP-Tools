using Verse;

namespace FCP.Core.Ghouls
{
    public class HediffComp_ToxicTransformation : HediffComp
    {
        private static HediffDef transformDef;

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);

            if (!Pawn.IsHashIntervalTick(600)) return;
            if (Pawn.IsGhoul()) return;

            if (transformDef == null)
                transformDef = DefDatabase<HediffDef>.GetNamed("FCP_Hediff_GhoulTransformation", false);

            var transform = Pawn.health.hediffSet.GetFirstHediffOfDef(transformDef);

            if (parent.Severity >= 0.85f && transform == null)
                Pawn.health.AddHediff(transformDef);
            else if (parent.Severity < 0.7f && transform != null)
                Pawn.health.RemoveHediff(transform);
        }
    }

    public class HediffCompProperties_ToxicTransformation : HediffCompProperties
    {
        public HediffCompProperties_ToxicTransformation()
        {
            compClass = typeof(HediffComp_ToxicTransformation);
        }
    }
}
