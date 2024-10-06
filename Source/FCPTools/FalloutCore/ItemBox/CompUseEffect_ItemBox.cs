using System.Collections.Generic;
using RimWorld;
using Verse;

namespace FCP.Core;

public class CompUseEffect_ItemBox : CompUseEffect
{
    public CompProperties_UseEffectItemBox Props => (CompProperties_UseEffectItemBox)props;

    public override void DoEffect(Pawn usedBy)
    {
        base.DoEffect(usedBy);
        
        // If you just use a thing set maker instead
        if (Props.thingSetMakerDef != null)
        {
            List<Thing> setDropList = Props.thingSetMakerDef.root.Generate();
            foreach (Thing thing in setDropList)
            {
                DropThing(thing);
            }
        }

        // Will attempt to drop everything in 
        if (!Props.guaranteedDrops.NullOrEmpty())
        {
            foreach (ItemDrop drop in Props.guaranteedDrops)
            {
                if (Rand.Chance(drop.chance))
                {
                    int count = drop.countRange.RandomInRange;
                    if (count > 0)
                    {
                        DoDrop(drop.thingDef, count);
                    }
                }
            }
        }

        // Will do X amount of drops, item randomly selected based on weight
        if (!Props.weightedDrops.NullOrEmpty())
        {
            for (int i = 0; i < Props.numWeightedDrops; i++)
            {
                ItemDrop drop = Props.weightedDrops.RandomElementByWeight(x => x.weight);
                int count = drop.countRange.RandomInRange;
                if (count > 0)
                {
                    DoDrop(drop.thingDef, count);
                }
            }
        }
    }

    private void DoDrop(ThingDef thingDef, int stackCount)
    {
        Thing droppedThing = ThingMaker.MakeThing(thingDef);
        droppedThing.stackCount = stackCount;
        droppedThing.TryGetComp<CompQuality>()?.SetQuality(GetRandomQuality(), ArtGenerationContext.Colony);
        DropThing(droppedThing);
    }

    private static QualityCategory GetRandomQuality()
    {
        var randomQuality = QualityUtility.GenerateQualityTraderItem();
        if (Rand.Chance(0.025f))
        {
            randomQuality = QualityCategory.Legendary;
        }

        return randomQuality;
    }

    private void DropThing(Thing thing)
    {
        GenPlace.TryPlaceThing(thing, parent.Position, parent.Map, ThingPlaceMode.Near);
    }
}