using HarmonyLib;
using UnityEngine;
using Verse;

namespace taranchuk_homingprojectiles
{
    [HarmonyPatch(typeof(Projectile), "Impact")]
    public static class Projectile_Impact_Patch
    {
        public static bool Prefix(Projectile __instance, ref Thing hitThing)
        {
            if (__instance.IsHomingProjectile(out var comp))
            {
                if (hitThing != __instance.intendedTarget.Thing)
                {
                    foreach (var thing in GenRadial.RadialDistinctThingsAround(__instance.Position, __instance.Map, 3f, true))
                    {
                        if (thing == __instance.intendedTarget.Thing)
                        {
                            if (Vector3.Distance(thing.DrawPos.Yto0(), __instance.ExactPosition.Yto0()) <= 0.5f)
                            {
                                hitThing = thing;
                            }
                        }
                    }
                }
            }
            return true;
        }
    }
}
