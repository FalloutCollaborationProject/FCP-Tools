namespace FCP.Tents;

public class HediffComp_RemoveUponGettingUp : HediffComp
{
    public override void CompPostTick(ref float severityAdjustment)
    {
        if (parent?.pawn?.CurrentBed()?.def?.HasModExtension<TentModExtension>() != true) 
            parent.pawn.health.RemoveHediff(parent);
    }

}