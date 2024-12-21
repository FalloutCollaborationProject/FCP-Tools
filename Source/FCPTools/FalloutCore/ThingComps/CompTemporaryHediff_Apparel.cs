namespace FCP.Core;

public class CompTemporaryHediff_Apparel : CompCauseHediff_Apparel
{
    public bool HediffGiven = false;
    public virtual bool PrereqsComplete => Props.projectRequired?.IsFinished ?? true;

    public CompProperties_TemporaryHediff_Apparel Props => (CompProperties_TemporaryHediff_Apparel)props;

    public override void Notify_Unequipped(Pawn pawn)
    {
        Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(Props.hediff);

        if (hediff != null)
        {
            pawn.health.RemoveHediff(hediff);
        }

        HediffGiven = false;
    }

    public override void CompTickLong()
    {
        base.CompTickRare();
        if (!HediffGiven && PrereqsComplete)
        {
            AddHediff(ParentHolder.ParentHolder as Pawn);
        }
    }

    public virtual void AddHediff(Pawn pawn)
    {
        if (pawn.health.hediffSet.GetFirstHediffOfDef(Props.hediff) != null)
        {
            return;
        }

        HediffComp_RemoveIfApparelDropped comp = pawn
            .health.AddHediff(Props.hediff, pawn.health.hediffSet.GetNotMissingParts().FirstOrFallback(p => p.def == Props.part))
            .TryGetComp<HediffComp_RemoveIfApparelDropped>();
        if (comp == null)
        {
            return;
        }

        comp.wornApparel = (Apparel)parent;
        HediffGiven = true;
    }

    public override void Notify_Equipped(Pawn pawn)
    {
        if (PrereqsComplete)
        {
            AddHediff(pawn);
        }
    }
}
