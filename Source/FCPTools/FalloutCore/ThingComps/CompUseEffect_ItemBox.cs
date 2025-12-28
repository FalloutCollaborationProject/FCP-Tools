using System.Collections.Generic;
using FCP.Core;
using RimWorld;
using Verse;

public class CompUseEffect_ItemBox : CompUseEffect
{
    public CompProperties_UseEffectItemBox Props => (CompProperties_UseEffectItemBox)props;

    private bool lootClaimed;

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Values.Look(ref lootClaimed, "lootClaimed", false);
    }

    public override void DoEffect(Pawn usedBy)
    {
        base.DoEffect(usedBy);

        if (lootClaimed)
        {
            return;
        }

        lootClaimed = true;
        DropLoot(parent.Position, parent.Map);
    }

    public override void PostDestroy(DestroyMode mode, Map previousMap)
    {
        base.PostDestroy(mode, previousMap);

        // Only drop on damage-based destruction.
        if (mode != DestroyMode.KillFinalize)
        {
            return;
        }

        if (lootClaimed)
        {
            return;
        }

        lootClaimed = true;

        Map map = previousMap;
        if (map == null)
        {
            return;
        }

        DropLoot(parent.Position, map);
    }

    private void DropLoot(IntVec3 pos, Map map)
    {
        if (map == null)
        {
            return;
        }

        if (Props.thingSetMakerDef != null)
        {
            List<Thing> list = Props.thingSetMakerDef.root.Generate();
            for (int i = 0; i < list.Count; i++)
            {
                DropThing(list[i], pos, map);
            }
        }

        if (!Props.guaranteedDrops.NullOrEmpty())
        {
            for (int i = 0; i < Props.guaranteedDrops.Count; i++)
            {
                ItemDropConfig guaranteedDrop = Props.guaranteedDrops[i];
                if (Rand.Chance(guaranteedDrop.chance))
                {
                    int count = guaranteedDrop.countRange.RandomInRange;
                    if (count > 0)
                    {
                        DoDrop(guaranteedDrop.thingDef, count, pos, map);
                    }
                }
            }
        }

        if (Props.weightedDrops.NullOrEmpty())
        {
            return;
        }

        for (int i = 0; i < Props.numWeightedDrops; i++)
        {
            ItemDropConfig itemDropConfig = Props.weightedDrops.RandomElementByWeight(x => x.weight);
            int count = itemDropConfig.countRange.RandomInRange;
            if (count > 0)
            {
                DoDrop(itemDropConfig.thingDef, count, pos, map);
            }
        }
    }

    private void DoDrop(ThingDef thingDef, int stackCount, IntVec3 pos, Map map)
    {
        Thing thing = ThingMaker.MakeThing(thingDef);
        thing.stackCount = stackCount;
        thing.TryGetComp<CompQuality>()?.SetQuality(GetRandomQuality(), ArtGenerationContext.Colony);
        DropThing(thing, pos, map);
    }

    private static QualityCategory GetRandomQuality()
    {
        QualityCategory result = QualityUtility.GenerateQualityTraderItem();
        if (Rand.Chance(0.025f))
        {
            result = QualityCategory.Legendary;
        }
        return result;
    }

    private void DropThing(Thing thing, IntVec3 pos, Map map)
    {
        GenPlace.TryPlaceThing(thing, pos, map, ThingPlaceMode.Near);
    }
}
