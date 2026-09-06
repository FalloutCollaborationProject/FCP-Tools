namespace FCP.Core.Hediffs;

public class HediffComp_HiddenButcherYield : HediffComp
{
    public HediffCompProperties_HiddenButcherYield Props =>
        (HediffCompProperties_HiddenButcherYield)props;

    public IEnumerable<Thing> MakeYieldThings()
    {
        foreach (ThingDefCountClass entry in Props.yield)
        {
            Thing thing = ThingMaker.MakeThing(entry.thingDef);
            thing.stackCount = entry.count;
            yield return thing;
        }
    }
}
