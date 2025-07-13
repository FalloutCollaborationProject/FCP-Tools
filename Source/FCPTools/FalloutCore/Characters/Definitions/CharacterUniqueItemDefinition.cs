using Verse;
using RimWorld;

namespace FCP.Core
{
    public class CharacterUniqueItemDefinition : CharacterBaseDefinition
    {
        public ThingDef uniqueItem;

        public override bool AppliesPreGeneration => false;
        public override bool AppliesPostGeneration => true;

        public override void ApplyToPawn(Pawn pawn)
        {
            if (uniqueItem == null)
            {
                return;
            }

            var tracker = UniqueCharactersTracker.Instance;
            if (tracker.IsUniqueThingCreated(uniqueItem))
            {
                return;
            }

            var item = ThingMaker.MakeThing(uniqueItem, uniqueItem.MadeFromStuff ? GenStuff.RandomStuffFor(uniqueItem) : null);
            if (item is Apparel apparel)
            {
                if (pawn.apparel != null && apparel.PawnCanWear(pawn))
                {
                    pawn.apparel.Wear(apparel, dropReplacedApparel: false);
                }
            }
            else if (item is ThingWithComps weapon && weapon.def.IsWeapon)
            {
                if (pawn.equipment != null && pawn.equipment.Primary == null)
                {
                    pawn.equipment.AddEquipment(weapon);
                }
            }
            else
            {
                if (!pawn.inventory.innerContainer.TryAdd(item))
                {
                    GenPlace.TryPlaceThing(item, pawn.Position, pawn.Map, ThingPlaceMode.Near);
                }
            }
        }
    }
}
