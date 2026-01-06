using HarmonyLib;
using Verse.AI;

namespace RangerRick_PowerArmor
{
    [HarmonyPatch(typeof(JobGiver_GetRest), "TryGiveJob")]
    public static class JobGiver_GetRest_Patch
    {
        public static bool Prefix(Pawn pawn, ref Job __result)
        {
            if (pawn?.apparel?.WornApparel == null) return true;

            foreach (var apparel in pawn.apparel.WornApparel)
            {
                var props = apparel.def.GetCompProperties<CompProperties_PowerArmor>();
                if (props != null && props.canSleep == false)
                {
                    __result = null;
                    return false;
                }
            }
            return true;
        }
    }
}
