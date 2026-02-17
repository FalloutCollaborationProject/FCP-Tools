namespace FCP.Core.LegendaryEffectWorkers;

public class IncendiaryWorker : LegendaryEffectWorker
{
    public override void Notify_ApplyToPawn(ref DamageInfo damageInfo, Pawn pawn)
    {
        if (pawn == null || damageInfo.IntendedTarget == null)
            return;
        
        if (damageInfo.IntendedTarget.CanEverAttachFire())
        {
            damageInfo.IntendedTarget.TryAttachFire(2, damageInfo.Instigator);
        }
    }
}
