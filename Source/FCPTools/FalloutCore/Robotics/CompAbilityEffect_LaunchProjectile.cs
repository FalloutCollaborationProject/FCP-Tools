using RimWorld;
using Verse;

namespace FCP.Core.Robotics
{
    public class CompProperties_AbilityLaunchProjectile : CompProperties_AbilityEffect
    {
        public ThingDef projectileDef;

        public CompProperties_AbilityLaunchProjectile()
        {
            compClass = typeof(CompAbilityEffect_LaunchProjectile);
        }
    }

    public class CompAbilityEffect_LaunchProjectile : CompAbilityEffect
    {
        public new CompProperties_AbilityLaunchProjectile Props => (CompProperties_AbilityLaunchProjectile)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);

            if (Props.projectileDef == null)
            {
                return;
            }

            Pawn pawn = parent.pawn;
            Projectile projectile = (Projectile)GenSpawn.Spawn(ThingMaker.MakeThing(Props.projectileDef), pawn.Position, pawn.Map);
            projectile.Launch(pawn, pawn.DrawPos, target, target, ProjectileHitFlags.IntendedTarget);
        }
    }
}
