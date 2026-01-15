using HarmonyLib;
using UnityEngine;
using Verse;

namespace taranchuk_homingprojectiles
{
    [HotSwappable]
    [HarmonyPatch(typeof(Projectile), nameof(Projectile.Tick))]
    public static class Projectile_Tick_Patch
    {
        public static void Postfix(Projectile __instance)
        {
            if (__instance.IsHomingProjectile(out var comp))
            {
                if (comp.CanChangeTrajectory(out bool delayTurning))
                {
                    //if (comp.RotateTowards(__instance.intendedTarget.CenterVector3, out var destination))
                    //{
                    //    __instance.SetDestination(destination);
                    //}
                    __instance.SetDestination(__instance.intendedTarget.CenterVector3);
                }
                else if (delayTurning && Vector3.Distance(__instance.ExactPosition.Yto0(), __instance.destination.Yto0()) <= 3)
                {
                    var offset = (Vector3.forward * 3f).RotatedBy(__instance.ExactRotation.eulerAngles.y);
                    var newDest = __instance.ExactPosition + offset;
                    __instance.SetDestination(newDest);
                }
                if (__instance.Destroyed is false && comp.Props.lifetimeTicks > 0 & Find.TickManager.TicksGame - comp.launchTick > comp.Props.lifetimeTicks)
                {
                    __instance.ImpactSomething();
                }
            }
        }
    }
}
