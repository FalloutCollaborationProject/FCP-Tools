using System.Reflection;

namespace FCP.Core.LegendaryEffectWorkers;

public class CripplingWorker : LegendaryEffectWorker
{
    public static Lazy<FieldInfo> DamageInfo_AmountInt = new(() => typeof(DamageInfo).GetField("amountInt", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic));

    public override void Notify_ApplyToPawn(ref DamageInfo damageInfo, Pawn pawn)
    {
        if (
            damageInfo.HitPart.def.defName.ToLower().Contains("arm")
            || damageInfo.HitPart.def.defName.ToLower().Contains("leg")
            || damageInfo.HitPart.def.defName.ToLower().Contains("tentacle")
            || damageInfo.HitPart.def.defName.ToLower().Contains("leg")
            || damageInfo.HitPart.def.defName.ToLower().Contains("leg")
        )
        {
            if (DamageInfo_AmountInt.Value != null)
            {
                float damageAmount = (float)DamageInfo_AmountInt.Value.GetValue(damageInfo);
                DamageInfo_AmountInt.Value.SetValueDirect(__makeref(damageInfo), damageAmount * 1.5f);
            }
        }
    }
}
