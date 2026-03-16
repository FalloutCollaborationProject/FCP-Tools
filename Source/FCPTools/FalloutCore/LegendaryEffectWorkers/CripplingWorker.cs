using HarmonyLib;

namespace FCP.Core.LegendaryEffectWorkers;

public class CripplingWorker : LegendaryEffectWorker
{
    public static readonly AccessTools.FieldRef<DamageInfo, float> DamageInfo_amountInt =
        AccessTools.FieldRefAccess<DamageInfo, float>("amountInt");

    public override void Notify_ApplyToPawn(ref DamageInfo damageInfo, Pawn pawn)
    {
        if (!ContainsLimb(damageInfo)) 
            return;

        float damageAmount = DamageInfo_amountInt(damageInfo); //(float)DamageInfo_AmountInt.Value.GetValue(damageInfo);
        DamageInfo_amountInt(damageInfo) = damageAmount * 1.5f;
    }

    private static bool ContainsLimb(DamageInfo damageInfo)
    {
        return damageInfo.HitPart.def.defName.ToLower().Contains("arm")
               || damageInfo.HitPart.def.defName.ToLower().Contains("tentacle")
               || damageInfo.HitPart.def.defName.ToLower().Contains("leg");
    }
}