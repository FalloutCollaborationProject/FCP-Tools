using HarmonyLib;

namespace FCP.Core;

[HarmonyPatch(typeof(Thing), nameof(Thing.Destroy))]
public static class Thing_Destroy_Patch
{
    public static void Prefix(Thing __instance)
    {
        if (__instance.def.HasModExtension<UniqueThingExtension>())
        {
            FCPLog.Warning($"Destroying unique thing {__instance} (holder: {__instance.ParentHolder})");
            UniqueCharactersTracker.Instance.Notify_UniqueThingDestroyed(__instance.def);
        }
    }
}