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
            if (!Props.items.NullOrEmpty())
            {
                foreach (ItemDrop drop in Props.items)
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
