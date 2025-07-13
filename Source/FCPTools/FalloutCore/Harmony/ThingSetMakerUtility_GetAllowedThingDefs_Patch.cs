using HarmonyLib;

namespace FCP.Core
{
    [HarmonyPatch(typeof(ThingSetMakerUtility), "GetAllowedThingDefs")]
    public static class ThingSetMakerUtility_GetAllowedThingDefs_Patch
    {
        public static IEnumerable<ThingDef> Postfix(IEnumerable<ThingDef> __result)
        {
            foreach (var thingDef in __result)
            {
                if (thingDef.IsUniqueItemAndCreatedAlready())
                {
                    continue;
                }
                yield return thingDef;
            }
        }
    }
}
