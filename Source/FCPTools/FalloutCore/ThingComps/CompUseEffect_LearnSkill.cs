namespace FCP.Core;

public class CompUseEffect_LearnSkill : CompUseEffect
{
    public CompProperties_UseEffect_LearnSkill Props => (CompProperties_UseEffect_LearnSkill)props;

    public override void DoEffect(Pawn usedBy)
    {
        base.DoEffect(usedBy);
        usedBy.skills?.Learn(Props.skill, Props.xpGainAmount, direct: true);
    }
}
