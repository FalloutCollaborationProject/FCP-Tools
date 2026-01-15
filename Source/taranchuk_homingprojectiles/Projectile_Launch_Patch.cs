using HarmonyLib;
using System;
using UnityEngine;
using Verse;

namespace taranchuk_homingprojectiles
{
    [HarmonyPatch(typeof(Projectile), "Launch", new Type[]
    {
        typeof(Thing),
        typeof(Vector3),
        typeof(LocalTargetInfo),
        typeof(LocalTargetInfo),
        typeof(ProjectileHitFlags),
        typeof(bool),
        typeof(Thing),
        typeof(ThingDef)
    })]
    public static class Projectile_Launch_Patch
    {

        public static void Postfix(Projectile __instance, Thing launcher, Vector3 origin, ref LocalTargetInfo usedTarget,
            LocalTargetInfo intendedTarget, bool preventFriendlyFire, Thing equipment, ThingDef targetCoverDef)
        {
            if (__instance.IsHomingProjectile(out var comp))
            {
                __instance.usedTarget = __instance.intendedTarget;
                __instance.SetDestination(__instance.intendedTarget.CenterVector3 + comp.DispersionOffset);
                comp.originLaunchCell = __instance.origin;
                comp.launchTick = Find.TickManager.TicksGame;
            }
        }


        public static void SetDestination(this Projectile projectile, Vector3 destination)
        {
            var projDestination = projectile.destination;
            float distanceBetweenDestinations = Vector3.Distance(projDestination.Yto0(), destination.Yto0());
            if (distanceBetweenDestinations >= 0.1f)
            {
                Vector3 origin = projectile.origin;
                Vector3 newPos = new Vector3(projectile.ExactPosition.x, origin.y, projectile.ExactPosition.z);
                if (projectile.Map is null || newPos.InBounds(projectile.Map))
                {
                    projectile.origin = newPos;
                    projectile.destination = destination;
                    projectile.ticksToImpact = Mathf.CeilToInt(projectile.StartingTicksToImpact - 1);
                }
            }
        }

        public static bool IsHomingProjectile(this Projectile projectile, out CompHomingProjectile comp)
        {
            comp = projectile.GetComp<CompHomingProjectile>();
            return comp != null;
        }
    }
}
