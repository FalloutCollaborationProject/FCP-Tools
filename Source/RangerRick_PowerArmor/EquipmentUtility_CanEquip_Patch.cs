using HarmonyLib;
using RimWorld;
using System;
using System.Linq;
using Verse;

namespace RangerRick_PowerArmor
{

    [HarmonyPatch(typeof(EquipmentUtility), "CanEquip", [typeof(Thing), typeof(Pawn), typeof(string), typeof(bool)],
        [ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Out, ArgumentType.Normal])]
    public static class EquipmentUtility_CanEquip_Patch
    {
        private static void Postfix(ref bool __result, Thing thing, Pawn pawn, ref string cantReason, bool checkBonded = true)
        {
            if (pawn.apparel != null && thing is Apparel)
            {
                var comp = thing.TryGetComp<CompPowerArmor>();
                if (comp != null)
                {
                    if (comp.HasRequiredTrait(pawn) is false)
                    {
                        cantReason = "RR.RequiresTrait".Translate(comp.Props.requiredTrait.degreeDatas[0].label);
                        __result = false;
                        return;
                    }
                    if (comp.HasRequiredApparel(pawn) is false)
                    {
                        if (comp.Props.requiredApparels.Count == 1)
                        {
                            cantReason = "RR.RequiresApparel".Translate(comp.Props.requiredApparels[0].label);
                        }
                        else
                        {
                            cantReason = "RR.RequiresApparelsAnyOf".Translate(string.Join(", ", comp.Props.requiredApparels.Select(x => x.label)));
                        }
                        __result = false;
                        return;
                    }
                }
            }
        }
    }
}
