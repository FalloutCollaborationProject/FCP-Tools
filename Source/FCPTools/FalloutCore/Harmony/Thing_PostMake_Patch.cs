using HarmonyLib;
using Verse;

namespace FCP.Core;

[HarmonyPatch(typeof(Thing), nameof(Thing.PostMake))]
public static class Thing_PostMake_Patch
{
    public static void Postfix(Thing __instance)
    {
        if (__instance.def.HasModExtension<UniqueThingExtension>())
        {
            UniqueCharactersTracker.Instance.Notify_UniqueThingSpawned(__instance.def);
        }
        if (FCPCoreMod.Settings.General.consumableGrenades || __instance.stackCount <= 1) return;
        if (IsConsumableGrenade(__instance.def))
        {
            __instance.stackCount = 1;
        }
    }

    internal static bool IsConsumableGrenade(ThingDef def)
    {
        if (!def.IsWeapon || def.Verbs.NullOrEmpty()) return false;
        for (int i = 0; i < def.Verbs.Count; i++)
        {
            if (def.Verbs[i].verbClass == typeof(Verb_LaunchProjectileConsumable)) return true;
        }
        return false;
    }
}