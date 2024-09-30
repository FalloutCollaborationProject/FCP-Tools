using System.Collections.Generic;
using JetBrains.Annotations;
using RimWorld;

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