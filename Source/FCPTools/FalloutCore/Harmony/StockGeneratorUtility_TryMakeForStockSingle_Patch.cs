using HarmonyLib;

namespace FCP.Core
{
    [HarmonyPatch(typeof(StockGeneratorUtility), "TryMakeForStockSingle")]
    public static class StockGeneratorUtility_TryMakeForStockSingle_Patch
    {
        public static bool Prefix(ThingDef thingDef)
        {
            if (thingDef.IsUniqueItemAndCreatedAlready())
            {
                return false;
            }
            return true;
        }

        public static bool IsUniqueItemAndCreatedAlready( this ThingDef thingDef)
        {
            return thingDef.HasModExtension<UniqueThingExtension>() && UniqueCharactersTracker.Instance.IsUniqueThingCreated(thingDef);
        }
    }
}
