using HarmonyLib;
using Verse;
using RimWorld.Planet;

namespace FCP.Core
{
    [HarmonyPatch(typeof(Thing), nameof(Thing.Destroy))]
    public static class Thing_Destroy_Patch
    {
        public static void Prefix(Thing __instance)
        {
            if (__instance.def.HasModExtension<UniqueThingExtension>())
            {
                UniqueCharactersTracker.Instance.Notify_UniqueThingDestroyed(__instance.def);
            }
        }
    }
}
