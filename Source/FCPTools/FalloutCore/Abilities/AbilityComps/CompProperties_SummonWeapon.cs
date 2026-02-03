namespace FCP.Core;

public class CompProperties_SummonWeapon : CompProperties_AbilityEffect
{
    public ThingDef weapon;

    public CompProperties_SummonWeapon() => compClass = typeof(CompSummonWeapon);
}
