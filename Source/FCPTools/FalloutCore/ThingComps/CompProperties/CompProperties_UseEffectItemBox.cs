namespace FCP.Core;

public class CompProperties_UseEffectItemBox : CompProperties_UseEffect
{
    public CompProperties_UseEffectItemBox()
    {
        compClass = typeof(CompUseEffect_ItemBox);
    }

    public ThingSetMakerDef thingSetMakerDef;
    public float thingSetMakerChance = 1f;
    public List<ItemDropConfig> guaranteedDrops;
    public List<ItemDropConfig> weightedDrops;
    public int numWeightedDrops = 1;
}