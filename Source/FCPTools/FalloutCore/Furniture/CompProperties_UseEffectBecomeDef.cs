namespace FCP.Furniture;

public class CompProperties_UseEffectBecomeDef : CompProperties_UseEffect
{
    public CompProperties_UseEffectBecomeDef()
    {
        compClass = typeof(CompUseEffect_BecomeDef);
    }

    public ThingDef becomeDef;
}
