using HarmonyLib;
using RimWorld;
using Verse;

namespace FCP.Core.Birds;

[HarmonyPatchCategory("Birds")]
[HarmonyPatch(typeof(StatExtension), nameof(StatExtension.GetStatValue))]
public static class StatExtension_GetStatValue
{
    [HarmonyPriority(Priority.Last)]
    private static void Postfix(Thing thing, StatDef stat, ref float __result)
    {
        if (stat == StatDefOf.MoveSpeed && thing is Pawn pawn && pawn.IsFlyingPawn(out var comp) &&
            comp.isFlyingCurrently)
        {
            __result *= comp.Props.flyingMoveSpeedMultiplier;
        }
    }
}