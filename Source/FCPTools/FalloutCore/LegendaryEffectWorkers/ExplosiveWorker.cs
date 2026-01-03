namespace FCP.Core.LegendaryEffectWorkers;

public class ExplosiveWorker : LegendaryEffectWorker
{
    public override void Notify_ApplyToPawn(ref DamageInfo damageInfo, Pawn pawn)
    {
        if (pawn == null)
            return;
        Effecter eff = FCPDefOf.FCP_VATSLegendaryEffect_Explosive_Explosion.Spawn();
        damageInfo.Instigator.Map.effecterMaintainer.AddEffecterToMaintain(eff, damageInfo.IntendedTarget.Position.ToVector3().ToIntVec3(), 60);

        GenExplosion.DoExplosion(
            damageInfo.IntendedTarget.Position,
            damageInfo.Instigator.Map,
            4,
            DamageDefOf.Bomb,
            damageInfo.Instigator,
            ignoredThings: [damageInfo.Instigator],
            propagationSpeed: 0.5f
        );
    }
}
