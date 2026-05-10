using HarmonyLib;
using UnityEngine;

namespace FCP.Core;

[HotSwappable]
[HarmonyPatch(typeof(Projectile), "Tick")]
public static class Projectile_Tick_Patch
{
    public static void Postfix(Projectile __instance)
    {
        if (__instance.IsHomingProjectile(out var comp))
        {
            if (comp.CanChangeTrajectory(out bool delayTurning))
            {
                __instance.SetDestination(__instance.intendedTarget.CenterVector3);
            }
            else if (delayTurning)
            {
                var destination = Traverse.Create(__instance).Field("destination").GetValue<Vector3>();
                if (Vector3.Distance(__instance.ExactPosition.Yto0(), destination.Yto0()) <= 3)
                {
                    var offset = (Vector3.forward * 3f).RotatedBy(__instance.ExactRotation.eulerAngles.y);
                    var newDest = __instance.ExactPosition + offset;
                    __instance.SetDestination(newDest);
                }
            }
            
            if (!__instance.Destroyed && comp.Props.lifetimeTicks > 0 && 
                Find.TickManager.TicksGame - comp.launchTick > comp.Props.lifetimeTicks)
            {
                Traverse.Create(__instance).Method("ImpactSomething").GetValue();
            }
        }
    }
}
