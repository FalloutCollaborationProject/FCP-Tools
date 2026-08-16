using HarmonyLib;

namespace FCP.Core.Turrets;

[HarmonyPatch(typeof(PlaceWorker_ShowTurretRadius), nameof(PlaceWorker_ShowTurretRadius.AllowsPlacing))]
public static class PlaceWorker_ShowTurretRadius_NullGuard_Patch
{
    public static bool Prefix(BuildableDef checkingDef, IntVec3 loc, ref AcceptanceReport __result)
    {
        ThingDef gunDef = (checkingDef as ThingDef)?.building?.turretGunDef;
        VerbProperties verb = gunDef?.Verbs?.Find(v => v.verbClass == typeof(Verb_Shoot) || typeof(Verb_Spray).IsAssignableFrom(v.verbClass));

        if (verb == null)
        {
            __result = true;
            return false;
        }

        if (verb.range > 0f)
            GenDraw.DrawRadiusRing(loc, verb.range);
        if (verb.minRange > 0f)
            GenDraw.DrawRadiusRing(loc, verb.minRange);

        __result = true;
        return false;
    }
}
