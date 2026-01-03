namespace FCP.Core.LegendaryEffectWorkers;

public class PoisonersWorker : LegendaryEffectWorker
{
    public override void Notify_ApplyToPawn(ref DamageInfo damageInfo, Pawn pawn)
    {
        pawn.health.AddHediff(FCPDefOf.FCP_VATSPoisoning, null, damageInfo);
    }
}
