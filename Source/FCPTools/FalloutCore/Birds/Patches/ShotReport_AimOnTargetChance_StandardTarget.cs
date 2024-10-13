using HarmonyLib;

namespace FCP.Core.Birds;

[HarmonyPatchCategory("Birds")]
[HarmonyPatch(typeof(ShotReport), nameof(ShotReport.AimOnTargetChance_StandardTarget), MethodType.Getter)]
public class ShotReport_AimOnTargetChance_StandardTarget
{
    public static void Postfix(ref float __result, TargetInfo ___target)
    {
        if (___target.Thing is not Pawn pawn)
        {
            return;
        }

        var compFlying = pawn.TryGetComp<CompFlyingPawn>();
        if (compFlying is { isFlyingCurrently: true })
        {
            __result *= 1f - compFlying.Props.evadeChanceWhenFlying;
        }
    }
}