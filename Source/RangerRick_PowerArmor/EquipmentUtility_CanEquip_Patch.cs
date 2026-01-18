using HarmonyLib;

namespace RangerRick_PowerArmor
{

    [HarmonyPatch(typeof(EquipmentUtility), "CanEquip", [typeof(Thing), typeof(Pawn), typeof(string), typeof(bool)],
        [ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Out, ArgumentType.Normal])]
    public static class EquipmentUtility_CanEquip_Patch
    {
        private static void Postfix(ref bool __result, Thing thing, Pawn pawn, ref string cantReason, bool checkBonded = true)
        {
            if (!__result) return;

            if (pawn.apparel != null && thing is Apparel)
            {
                var reqComp = thing.TryGetComp<CompApparelRequirement>();

                if (reqComp != null)
                {
                    var acceptanceReport = reqComp.CanWear(pawn);
                    if (!acceptanceReport)
                    {
                        cantReason = acceptanceReport.Reason;
                        __result = false;
                        return;
                    }
                }
            }
        }
    }
}
