namespace FCP.Core;

public class CompProperties_BurrowAmbush : CompProperties_AbilityEffect
{
    public float maxBodySize = 3f;

    public CompProperties_BurrowAmbush() => compClass = typeof(CompAbilityEffect_BurrowAmbush);
}
