namespace FCP.Core.Hediffs;

public class HediffCompProperties_HiddenButcherYield : HediffCompProperties
{
    public List<ThingDefCountClass> yield = new();

    public bool replacesNormalYield;

    public HediffCompProperties_HiddenButcherYield() =>
        compClass = typeof(HediffComp_HiddenButcherYield);
}
