using HarmonyLib;
using RimWorld;
using Verse;

namespace FCP.Core.Birds;

[HarmonyPatchCategory("Birds")]
[HarmonyPatch(typeof(JobGiver_AIDefendPawn), "FindAttackTarget")]
public static class JobGiver_AIDefendPawn_FindAttackTarget
{
    public static void Postfix(JobGiver_AIDefendPawn __instance, ref Thing __result, Pawn pawn)
    {
        if (pawn == null || __instance is not JobGiver_AIDefendMaster)
        {
            return;
        }

        if (!pawn.IsFlyingPawn(out var flyingComp))
        {
            return;
        }

        if (!flyingComp.Props.attackEnemiesMasterAttacking)
        {
            return;
        }

        var defendee = pawn.playerSettings.Master;
        if (defendee.CurJobDef == JobDefOf.AttackStatic || defendee.CurJobDef == JobDefOf.AttackMelee)
        {
            __result = defendee.CurJob.targetA.Thing;
        }
    }
}