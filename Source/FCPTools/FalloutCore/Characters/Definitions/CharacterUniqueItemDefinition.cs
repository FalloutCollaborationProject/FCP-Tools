namespace FCP.Core;

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

        if (uniqueItem.IsApparel)
        {
            FCPLog.Warning($"CharacterUniqueItemDefinition: applying {uniqueItem.defName} to {pawn}");

            Apparel existing = pawn.apparel?.WornApparel.FirstOrDefault(a => a.def == uniqueItem);
            if (existing != null)
            {
                FCPLog.Warning($"CharacterUniqueItemDefinition: {pawn} already wearing {uniqueItem.defName}, locking");
                pawn.apparel.Lock(existing);
                return;
            }

            if (pawn.apparel == null)
            {
                FCPLog.Warning($"CharacterUniqueItemDefinition: {pawn} has no apparel tracker, cannot equip {uniqueItem.defName}");
                return;
            }

            var apparel = (Apparel)ThingMaker.MakeThing(uniqueItem, uniqueItem.MadeFromStuff ? GenStuff.RandomStuffFor(uniqueItem) : null);
            pawn.apparel.Wear(apparel, dropReplacedApparel: true);
            bool nowWorn = pawn.apparel.WornApparel.Contains(apparel);
            FCPLog.Warning($"CharacterUniqueItemDefinition: {pawn} Wear() called for {uniqueItem.defName}, now worn: {nowWorn}");
            if (nowWorn)
            {
                pawn.apparel.Lock(apparel);
            }
            return;
        }

        var tracker = UniqueCharactersTracker.Instance;
        if (tracker.IsUniqueThingCreated(uniqueItem))
        {
            return;
        }

        var item = ThingMaker.MakeThing(uniqueItem, uniqueItem.MadeFromStuff ? GenStuff.RandomStuffFor(uniqueItem) : null);
        if (item is ThingWithComps weapon && weapon.def.IsWeapon)
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