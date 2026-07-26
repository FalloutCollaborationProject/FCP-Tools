using Verse.AI;

namespace FCP.Core;

public class CompAbilityEffect_BurrowAmbush : CompAbilityEffect, ICompAbilityEffectOnJumpCompleted
{
    private new CompProperties_BurrowAmbush Props => (CompProperties_BurrowAmbush)props;

    public void OnJumpCompleted(IntVec3 origin, LocalTargetInfo target)
    {
        Pawn caster = parent.pawn;
        Map map = caster.Map;
        if (map != null)
        {
            FleckMaker.ThrowDustPuff(origin.ToVector3Shifted(), map, 2f);
            FleckMaker.ThrowDustPuff(caster.Position.ToVector3Shifted(), map, 2f);
        }
        if (target.Pawn is Pawn victim && !victim.Dead && !victim.Downed)
        {
            caster.jobs.StartJob(JobMaker.MakeJob(JobDefOf.AttackMelee, victim), JobCondition.InterruptForced);
        }
    }

    public override bool AICanTargetNow(LocalTargetInfo target)
    {
        Pawn pawn = target.Pawn;
        if (pawn == null)
        {
            return false;
        }
        return pawn.BodySize <= Props.maxBodySize;
    }

    public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
    {
        if (target.Pawn != null && target.Pawn.BodySize > Props.maxBodySize)
        {
            return false;
        }
        return base.Valid(target, throwMessages);
    }
}
