// Decompiled with JetBrains decompiler
// Type: RangerRick_PowerArmor.EquipmentUtility_CanEquip_Patch
// Assembly: RangerRick_PowerArmor, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2F9A23E8-63EA-4090-93FB-DECA3711786E
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\RimWorld\Mods\FCP-Tools\1.5\Assemblies\RangerRick_PowerArmor.dll

using FCP.Core;
using HarmonyLib;
using RimWorld;
using System;
using System.Linq;
using Verse;

#nullable disable
namespace FCP.Core
{
    [HarmonyPatch]
    public static class EquipmentUtility_CanEquip_Patch
    {
        private static void Postfix(
          ref bool __result,
          Thing thing,
          Pawn pawn,
          ref string cantReason,
          bool checkBonded = true)
        {
            if (pawn.apparel == null || !(thing is Apparel))
                return;
            CompPowerArmor comp = thing.TryGetComp<CompPowerArmor>();
            if (comp == null)
                return;
            if (!comp.HasRequiredTrait(pawn))
            {
                cantReason = (string)"RR.RequiresTrait".Translate((NamedArgument)comp.Props.requiredTrait.degreeDatas[0].label);
                __result = false;
            }
            else
            {
                if (comp.HasRequiredApparel(pawn))
                    return;
                cantReason = comp.Props.requiredApparels.Count != 1 ? (string)"RR.RequiresApparelsAnyOf".Translate((NamedArgument)string.Join(", ", comp.Props.requiredApparels.Select<ThingDef, string>((Func<ThingDef, string>)(x => x.label)))) : (string)"RR.RequiresApparel".Translate((NamedArgument)comp.Props.requiredApparels[0].label);
                __result = false;
            }
        }
    }
}
