namespace FCP.Core;

public class CompProperties_UseEffect_LearnSkill : CompProperties_UseEffect
{
    public CompProperties_UseEffect_LearnSkill()
    {
        compClass = typeof(CompUseEffect_LearnSkill);
    }

    public SkillDef skill;
    public float xpGainAmount;
}
