// Decompiled with JetBrains decompiler
// Type: RangerRick_PowerArmor.CompApparelDependency
// Assembly: RangerRick_PowerArmor, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2F9A23E8-63EA-4090-93FB-DECA3711786E
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\RimWorld\Mods\FCP-Tools\1.5\Assemblies\RangerRick_PowerArmor.dll

using RimWorld;
using System.Linq;
using Verse;

#nullable disable
namespace FCP.Core
{
    public class CompApparelDependency : ThingComp
    {
        public override void Notify_Unequipped(Pawn pawn)
        {
            base.Notify_Unequipped(pawn);
            foreach (Apparel ap in pawn.apparel.WornApparel.ToList())
            {
                if (pawn.apparel.WornApparel.Contains(ap))
                {
                    CompPowerArmor comp = ap.GetComp<CompPowerArmor>();
                    if (comp != null && comp.Props.requiredApparels != null && !comp.HasRequiredApparel(pawn))
                        pawn.apparel.TryDrop(ap);
                }
            }
        }
    }
}
