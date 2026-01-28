namespace FCP.WeaponRequirement;

public abstract class WeaponRequirement
{
    public virtual bool RequiresCheckingOnTick => false;
    public abstract bool RequirementMet(Pawn pawn, Thing equipment, bool onTick = false);
}