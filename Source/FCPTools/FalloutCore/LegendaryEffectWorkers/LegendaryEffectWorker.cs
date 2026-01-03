namespace FCP.Core.LegendaryEffectWorkers;

public class LegendaryEffectWorker
{
    public LegendaryWeaponTraitDef def;
    public virtual void Notify_ApplyToPawn(ref DamageInfo damageInfo, Pawn pawn)
    {
        if (pawn == null)
        {
            return;
        }
    }
}
