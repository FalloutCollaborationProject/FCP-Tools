using System.Reflection;
using UnityEngine;

namespace FCP.Core.LegendaryEffectWorkers;

public class BloodiedWorker : LegendaryEffectWorker
{
    public static Lazy<FieldInfo> DamageInfo_AmountInt = new(() => typeof(DamageInfo).GetField("amountInt", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic));

    public override void Notify_ApplyToPawn(ref DamageInfo damageInfo, Pawn pawn)
    {
        if (pawn != null)
        {
            float extraDamage = Math.Abs(20 - Mathf.Ceil(pawn.health.summaryHealth.SummaryHealthPercent * 20)) * 0.05f + 1f;

            if (DamageInfo_AmountInt.Value != null)
            {
                float damageAmount = (float)DamageInfo_AmountInt.Value.GetValue(damageInfo);
                DamageInfo_AmountInt.Value.SetValueDirect(__makeref(damageInfo), damageAmount * extraDamage);
            }
        }
    }
}
