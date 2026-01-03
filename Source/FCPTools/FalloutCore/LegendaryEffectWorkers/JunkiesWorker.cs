using System.Reflection;

namespace FCP.Core.LegendaryEffectWorkers;

public class JunkiesWorker : LegendaryEffectWorker
{
    public static Lazy<FieldInfo> DamageInfo_AmountInt = new(() => typeof(DamageInfo).GetField("amountInt", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic));

    public override void Notify_ApplyToPawn(ref DamageInfo damageInfo, Pawn pawn)
    {
        if (pawn != null)
        {
            int addictions = pawn.health.hediffSet.hediffs.Count(hediff => hediff.def.defName.ToLower().Contains("addict"));

            float extraDamage = addictions * 0.15f + 1f;

            if (DamageInfo_AmountInt.Value != null)
            {
                float damageAmount = (float)DamageInfo_AmountInt.Value.GetValue(damageInfo);
                DamageInfo_AmountInt.Value.SetValueDirect(__makeref(damageInfo), damageAmount * extraDamage);
            }
        }
    }
}
