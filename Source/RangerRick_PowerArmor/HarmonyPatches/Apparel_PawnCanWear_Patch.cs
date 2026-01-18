using HarmonyLib;

namespace RangerRick_PowerArmor
{
    [HarmonyPatch(typeof(Apparel), "PawnCanWear")]
    public static class Apparel_PawnCanWear_Patch
    {
        public static void Postfix(ref bool __result, Apparel __instance, Pawn pawn)
        {
            if (!__result) return;

            var reqComp = __instance.GetComp<CompApparelRequirement>();
            if (reqComp != null && !reqComp.CanWear(pawn))
            {
                __result = false;
            }
        }
    }
}
