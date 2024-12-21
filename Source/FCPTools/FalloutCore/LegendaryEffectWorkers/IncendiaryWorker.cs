namespace FCP.Core.LegendaryEffectWorkers;

public class IncendiaryWorker : LegendaryEffectWorker
{
    public override void ApplyEffect(ref DamageInfo damageInfo, Pawn pawn)
    {
        if (pawn != null && damageInfo.IntendedTarget.CanEverAttachFire())
        {
            damageInfo.IntendedTarget.TryAttachFire(2, damageInfo.Instigator);
        }
    }
}
