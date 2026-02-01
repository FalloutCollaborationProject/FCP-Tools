namespace FCP.Core;

public class CompProperties_SummonedWeapon : CompProperties
{
    public int lifetimeDuration;
    public FleckDef fleckWhenExpired;

    public CompProperties_SummonedWeapon() => compClass = typeof(CompSummonedWeapon);
}
