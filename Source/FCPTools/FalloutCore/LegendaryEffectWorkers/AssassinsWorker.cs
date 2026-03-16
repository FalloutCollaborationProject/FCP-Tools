using HarmonyLib;

namespace FCP.Core.LegendaryEffectWorkers;

public class AssassinsWorker : LegendaryEffectWorker
{
    public static readonly AccessTools.FieldRef<DamageInfo, float> DamageInfo_amountInt =
        AccessTools.FieldRefAccess<DamageInfo, float>("amountInt");

    public override void Notify_ApplyToPawn(ref DamageInfo damageInfo, Pawn pawn)
    {
        if (pawn == null)
            return;

        float damageAmount = DamageInfo_amountInt(damageInfo);
        DamageInfo_amountInt(damageInfo) = damageAmount * 1.5f;
    }
}