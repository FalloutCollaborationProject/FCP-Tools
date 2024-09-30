using RimWorld;
using Verse;

namespace FCP.Core
{
    public class CompUseEffect_ItemBox : CompUseEffect
    {
        public CompProperties_UseEffectItemBox Props => (CompProperties_UseEffectItemBox)props;

        public override void DoEffect(Pawn usedBy)
        {
            base.DoEffect(usedBy);
            
            if (Props.items.NullOrEmpty())
                return;
            
            foreach (var itemDrop in Props.items)
            {
                if (Rand.Chance(itemDrop.chance))
                {
                    int count = itemDrop.countRange.RandomInRange;
                    if (count > 0)
                    {
                        DoDrop(itemDrop.thingDef, count);
                    }
                }
            }
        }

        private void DoDrop(ThingDef thingDef, int stackCount)
        {
            var droppedThing = ThingMaker.MakeThing(thingDef);
            droppedThing.stackCount = stackCount;
            droppedThing.TryGetComp<CompQuality>()?.SetQuality(QualityUtility.GenerateQualityRandomEqualChance(), ArtGenerationContext.Colony);
            GenPlace.TryPlaceThing(droppedThing, parent.Position, parent.Map, ThingPlaceMode.Near);
        }
    }
}
