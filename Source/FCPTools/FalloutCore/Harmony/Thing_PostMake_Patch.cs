using HarmonyLib;
using Verse;

namespace FCP.Core
{
    [HarmonyPatch(typeof(Thing), nameof(Thing.PostMake))]
    public static class Thing_PostMake_Patch
    {
        public static void Postfix(Thing __instance)
        {
            if (__instance.def.HasModExtension<UniqueThingExtension>())
            {
                UniqueCharactersTracker.Instance.Notify_UniqueThingSpawned(__instance.def);
            }
        }
    }
}
