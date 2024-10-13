using HarmonyLib;

namespace FCP.Core.Birds;

[HarmonyPatchCategory("Birds")]
[HarmonyPatch(typeof(DamageWorker_AddInjury), "ApplyDamageToPart")]
public static class DamageWorker_AddInjury_ApplyDamageToPart
{
    public static void Prefix(ref DamageInfo dinfo, Pawn pawn, DamageWorker.DamageResult result)
    {
        var instigator = dinfo.Instigator;
        if (instigator is not Pawn instagatorPawn || !instagatorPawn.IsFlyingPawn(out var flyingComp))
        {
            return;
        }

        var attackDamage = flyingComp.Props.attackDamageFactor;
        if (attackDamage == null)
        {
            return;
        }

        if (pawn.BodySize <= attackDamage.targetBodySize)
        {
            dinfo.SetAmount(dinfo.Amount * attackDamage.damageMultiplier);
        }
    }
}