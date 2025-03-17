using System.Reflection;

namespace FCP.Core.LegendaryEffectWorkers;

public class CripplingWorker : LegendaryEffectWorker
{
    public override void ApplyEffect(ref DamageInfo damageInfo, Pawn pawn)
    {
        if (
            damageInfo.HitPart.def.defName.ToLower().Contains("arm")
            || damageInfo.HitPart.def.defName.ToLower().Contains("leg")
            || damageInfo.HitPart.def.defName.ToLower().Contains("tentacle")
            || damageInfo.HitPart.def.defName.ToLower().Contains("leg")
            || damageInfo.HitPart.def.defName.ToLower().Contains("leg")
        )
        {
            Type dType = typeof(DamageInfo);
            FieldInfo amountInt = dType.GetField("amountInt", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (amountInt != null)
            {
                float damageAmount = (float)amountInt.GetValue(damageInfo);
                amountInt.SetValueDirect(__makeref(damageInfo), damageAmount * 1.5f);
            }
        }
    }
}
