using RimWorld;
using Verse;

namespace Rick_ItemBox
{
    public class CompUseEffect_ItemBox : CompUseEffect
    {
        public CompProperties_UseEffectItemBox Props => (CompProperties_UseEffectItemBox)props;

        public override void DoEffect(Pawn usedBy)
        {
            base.DoEffect(usedBy);
            ///Will attempt to drop everything in 
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
            ///Will do X amount of drops, item randomly selected based on weight
            if (!Props.weightedDrops.NullOrEmpty())
            {
                for (int i = 0; i < Props.numWeightedDrops; i++)
                {
                    ItemDrop drop = Props.weightedDrops.RandomElementByWeight(x=>x.weight);
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
            droppedThing.TryGetComp<CompQuality>()?.SetQuality(QualityUtility.GenerateQualityRandomEqualChance(), ArtGenerationContext.Colony);
            GenPlace.TryPlaceThing(droppedThing, parent.Position, parent.Map, ThingPlaceMode.Near);
        }
    }
}
