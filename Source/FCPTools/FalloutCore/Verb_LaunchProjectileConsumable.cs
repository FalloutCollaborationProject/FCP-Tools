namespace FCP.Core;

public class Verb_LaunchProjectileConsumable : Verb_LaunchProjectile
{
    protected override bool TryCastShot()
    {
        if (!base.TryCastShot()) return false;
        if (!FCPCoreMod.Settings.General.consumableGrenades) return true;
        if (EquipmentSource == null || EquipmentSource.Destroyed) return true;
        
        if (EquipmentSource.stackCount > 1)
        {
            EquipmentSource.stackCount--;
        }
        else
        {
            Thing replacement = FindReplacementInInventory();
            EquipmentSource.Destroy();
            if (replacement is ThingWithComps equipment)
            {
                CasterPawn?.equipment?.AddEquipment(equipment);
            }
        }
        return true;
    }
    
    private Thing FindReplacementInInventory()
    {
        if (CasterPawn?.inventory?.innerContainer == null) return null;
        for (int i = 0; i < CasterPawn.inventory.innerContainer.Count; i++)
        {
            Thing thing = CasterPawn.inventory.innerContainer[i];
            if (thing.def == EquipmentSource.def)
            {
                CasterPawn.inventory.innerContainer.Remove(thing);
                return thing;
            }
        }
        return null;
    }
}
