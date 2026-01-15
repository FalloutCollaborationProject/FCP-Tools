using RimWorld;
using Verse;

namespace BM_WeaponSummon
{
    public class CompProperties_SummonWeapon : CompProperties_AbilityEffect
    {
        public ThingDef weapon;
        public CompProperties_SummonWeapon() => compClass = typeof(CompSummonWeapon);
    }

    public class CompSummonWeapon : CompAbilityEffect
    {
        public CompProperties_SummonWeapon Props => base.props as CompProperties_SummonWeapon;

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
}
