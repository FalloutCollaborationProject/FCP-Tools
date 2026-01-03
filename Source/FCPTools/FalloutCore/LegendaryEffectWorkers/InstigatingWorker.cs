using System.Reflection;
using UnityEngine;

namespace FCP.Core.LegendaryEffectWorkers;

public class InstigatingWorker : LegendaryEffectWorker
{
    public override void Notify_ApplyToPawn(ref DamageInfo damageInfo, Pawn pawn)
    {
        if (pawn != null && Mathf.Approximately(pawn.health.summaryHealth.SummaryHealthPercent, 0f))
        {
            Type dType = typeof(DamageInfo);
            FieldInfo amountInt = dType.GetField("amountInt", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (amountInt != null)
            {
                float damageAmount = (float)amountInt.GetValue(damageInfo);
                amountInt.SetValueDirect(__makeref(damageInfo), damageAmount * 2f);
            }
        }
    }
}
