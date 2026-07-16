using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace FCP.Core
{
    [HarmonyPatch(typeof(CompProperties_Refuelable), nameof(CompProperties_Refuelable.SpecialDisplayStats))]
    public static class CompProperties_Refuelable_SpecialDisplayStats_Patch
    {
        public static bool Prefix(StatRequest req, ref IEnumerable<StatDrawEntry> __result)
        {
            if (req.Def is ThingDef thingDef && thingDef.building != null)
            {
                return true;
            }

            __result = Enumerable.Empty<StatDrawEntry>();
            return false;
        }
    }
}
