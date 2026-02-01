namespace FCP.Core;

public class JobGiver_AICastSummonWeapon : JobGiver_AICastAbility
{
    protected override LocalTargetInfo GetTarget(Pawn caster, Ability ability)
    {
        var existingWeapon = caster.equipment?.Primary;
        if (existingWeapon != null)
        {
            var comp = existingWeapon.GetComp<CompSummonedWeapon>();
            if (comp is null)
            {
                return caster;
            }
        }
        return LocalTargetInfo.Invalid;
    }
}
