namespace FCP.Core.LegendaryEffectWorkers;

public class LegendaryEffectWorker
{
    public LegendaryEffectDef effect;

    public void InitSetEffect(LegendaryEffectDef newEffect) => this.effect = newEffect;

    public virtual void ApplyEffect(ref DamageInfo damageInfo, Pawn pawn)
    {
        // 20% of the time
        if (pawn == null)
        {
            return;
        }

        if (effect.hediffToApply != null)
        {
            pawn.health.AddHediff(effect.hediffToApply, dinfo: damageInfo);
        }

        if (effect.Stuns)
        {
            pawn.stances.stunner.StunFor(effect.StunDuration, damageInfo.Instigator);
        }
    }
}
