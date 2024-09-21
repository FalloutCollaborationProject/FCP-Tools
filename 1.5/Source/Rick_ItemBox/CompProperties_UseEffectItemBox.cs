using RimWorld;
using System.Collections.Generic;
using Verse;

namespace Rick_ItemBox
{
    public class CompProperties_UseEffectItemBox : CompProperties_UseEffect
    {
        public CompProperties_UseEffectItemBox()
        {
            compClass = typeof(CompUseEffect_ItemBox);
        }

        public List<ItemDrop> items;
    }

    public class ItemDrop
    {
        public ThingDef thingDef;
        public IntRange countRange = new IntRange(1,1);
        public float chance = 1f;
    }
}
