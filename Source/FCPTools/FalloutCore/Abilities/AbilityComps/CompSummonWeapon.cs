namespace FCP.Core;

public class CompSummonWeapon : CompAbilityEffect
{
    public new CompProperties_SummonWeapon Props => props as CompProperties_SummonWeapon;

    public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
    {
        base.Apply(target, dest);
        var existingWeapon = parent.pawn.equipment.Primary;
        if (existingWeapon != null)
        {
            parent.pawn.equipment.TryTransferEquipmentToContainer(existingWeapon, parent.pawn.inventory.innerContainer);
        }
        var newWeapon = ThingMaker.MakeThing(Props.weapon) as ThingWithComps;
        var comp = newWeapon.TryGetComp<CompSummonedWeapon>();
        comp.ticksSummoned = Find.TickManager.TicksGame;
        parent.pawn.equipment.AddEquipment(newWeapon);
    }
}
