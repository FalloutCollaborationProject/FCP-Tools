namespace FCP.Core.Turrets;

public class CompProperties_TurretWeaponMount : CompProperties
{
    public CompProperties_TurretWeaponMount()
    {
        compClass = typeof(CompTurretWeaponMount);
    }

    public ThingDef emptyGunDef;
    public int componentCost = 2;
    public float powerConsumptionUnarmed = 20f;
    public float powerConsumptionArmed = 60f;
    public int installWorkTicks = 180;
}
