using HarmonyLib;
using RimWorld.Planet;

namespace FCP.Core.Shuttles;

public class CompProperties_AtmosphericOnly : CompProperties
{
    public CompProperties_AtmosphericOnly()
    {
        compClass = typeof(CompAtmosphericOnly);
    }
}

public class CompAtmosphericOnly : ThingComp
{
}

[HarmonyPatch(typeof(CompLaunchable), nameof(CompLaunchable.TryLaunch))]
public static class CompLaunchable_TryLaunch_AtmosphericOnly_Patch
{
    public static bool Prefix(CompLaunchable __instance, PlanetTile __0)
    {
        if (!__0.Valid || !__0.LayerDef.isSpace || !__instance.parent.HasComp<CompAtmosphericOnly>())
            return true;

        Messages.Message("FCP_CannotLaunchToOrbit".Translate(), MessageTypeDefOf.RejectInput, false);
        return false;
    }
}
