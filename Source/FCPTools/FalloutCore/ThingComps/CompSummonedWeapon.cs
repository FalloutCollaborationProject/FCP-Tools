namespace FCP.Core;

public class CompSummonedWeapon : ThingComp
{
    public int ticksSummoned;

    public CompProperties_SummonedWeapon Props => props as CompProperties_SummonedWeapon;
    public Pawn_EquipmentTracker EquipmentTracker => parent?.ParentHolder as Pawn_EquipmentTracker;

    // Todo this could probably be further optimized.
    public override void CompTick()
    {
        base.CompTick();

        if (Find.TickManager.TicksGame - ticksSummoned < Props.lifetimeDuration && EquipmentTracker is not null) 
            return;
        
        Map mapHeld = parent.MapHeld;
        if (mapHeld != null && Props.fleckWhenExpired != null)
        {
            FleckMaker.Static(parent.PositionHeld, mapHeld, Props.fleckWhenExpired);
        }
        parent.Destroy();
            
        Thing existingWeapon = EquipmentTracker?.pawn.inventory.innerContainer?.FirstOrDefault(x => x.def.IsWeapon);
        if (existingWeapon is ThingWithComps weapon)
        {
            existingWeapon.holdingOwner?.Remove(weapon);
            EquipmentTracker.AddEquipment(weapon);
        }
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Values.Look(ref ticksSummoned, "ticksSummoned");
    }
}
