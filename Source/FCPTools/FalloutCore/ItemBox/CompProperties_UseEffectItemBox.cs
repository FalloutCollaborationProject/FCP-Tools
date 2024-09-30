using System.Collections.Generic;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace FCP.Core;

[UsedImplicitly]
public class CompProperties_UseEffectItemBox : CompProperties_UseEffect
{
    public CompProperties_UseEffectItemBox()
    {
        compClass = typeof(CompUseEffect_ItemBox);
    }

    public List<ItemDrop> items;
}

[UsedImplicitly]
public class ItemDrop
{
    public ThingDef thingDef;
    public IntRange countRange = new IntRange(1,1);
    public float chance = 1f;
}