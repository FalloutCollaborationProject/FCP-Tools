namespace FCP.Core;

public class CompProperties_SummonedWeapon : CompProperties
{
    public int lifetimeDuration;
    public FleckDef fleckWhenExpired;

    public CompProperties_SummonedWeapon() => compClass = typeof(CompSummonedWeapon);

    public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
    {
        if (parentDef.tickerType != TickerType.Normal)
            yield return $"CompProperties_SummonedWeapon requires TickerType.Normal on {parentDef.defName}";
        
        foreach (var error in base.ConfigErrors(parentDef))
            yield return error;
    }
}