using HarmonyLib;
using RimWorld;
using Verse;

namespace FCP.Core.Robotics
{
    [HarmonyPatch(typeof(CompRefuelable), nameof(CompRefuelable.PostDraw))]
    public static class CompRefuelable_PostDraw_Patch
    {
        public static bool Prefix(CompRefuelable __instance)
        {
            Pawn pawn = __instance.parent as Pawn;
            return pawn == null || !pawn.Dead;
        }
    }
}
