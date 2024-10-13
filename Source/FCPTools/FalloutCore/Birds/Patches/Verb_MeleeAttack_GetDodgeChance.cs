using HarmonyLib;

namespace FCP.Core.Birds;

[HarmonyPatchCategory("Birds")]
[HarmonyPatch(typeof(Verb_MeleeAttack), "GetDodgeChance")]
public static class Verb_MeleeAttack_GetDodgeChance
{
    public static void Postfix(ref float __result, LocalTargetInfo target)
    {
        var pawn = target.Pawn;
        if (!pawn.IsFlyingPawn(out var flyingComp) || !flyingComp.isFlyingCurrently)
        {
            return;
        }

        __result *= flyingComp.Props.evadeChanceWhenFlying;
    }
}