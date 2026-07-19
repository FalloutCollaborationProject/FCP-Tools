using HarmonyLib;
using Verse;

namespace FCP.Core.Robotics
{
    [HarmonyPatch(typeof(PawnRenderUtility), nameof(PawnRenderUtility.CarryWeaponOpenly))]
    public static class CarryWeaponOpenly_Patch
    {
        public static void Postfix(Pawn pawn, ref bool __result)
        {
            if (!__result && RobotUtility.IsAnyRobot(pawn))
            {
                __result = true;
            }
        }
    }
}
