namespace FCP.Core;

public class Verb_Shoot_Syringer : Verb_Shoot
{
    private Comp_SyringerAmmo AmmoComp => EquipmentSource?.GetComp<Comp_SyringerAmmo>();

    public override ThingDef Projectile
    {
        get
        {
            ThingDef loadedSyringeDef = AmmoComp?.LoadedSyringeDef;
            DefExtension_SyringeAmmo ext = loadedSyringeDef?.GetModExtension<DefExtension_SyringeAmmo>();
            if (ext?.projectile != null)
            {
                return ext.projectile;
            }
            return base.Projectile;
        }
    }

    public override bool Available()
    {
        if (!base.Available())
        {
            return false;
        }
        return AmmoComp?.LoadedSyringeDef != null;
    }

    protected override bool TryCastShot()
    {
        bool result = base.TryCastShot();
        if (result)
        {
            AmmoComp?.Notify_ShotFired();
        }
        return result;
    }
}
