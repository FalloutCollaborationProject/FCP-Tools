using RimWorld;
using Verse;

namespace BM_WeaponSummon
{
    public class JobGiver_AICastSummonWeapon : JobGiver_AICastAbility
    {
        public override LocalTargetInfo GetTarget(Pawn caster, Ability ability)
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
}
