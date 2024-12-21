using System.Reflection;
using UnityEngine;

namespace FCP.Core.LegendaryEffectWorkers;

public class BloodiedWorker : LegendaryEffectWorker
{
    public override void ApplyEffect(ref DamageInfo damageInfo, Pawn pawn)
    {
        if (pawn != null)
        {
            float extraDamage = Math.Abs(20 - Mathf.Ceil(pawn.health.summaryHealth.SummaryHealthPercent * 20)) * 0.05f + 1f;

            Type dType = typeof(DamageInfo);
            FieldInfo amountInt = dType.GetField("amountInt", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (amountInt != null)
            {
                float damageAmount = (float)amountInt.GetValue(damageInfo);
                amountInt.SetValueDirect(__makeref(damageInfo), damageAmount * extraDamage);
            }
        }
    }
}
