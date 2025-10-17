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
                var reqComp = thing.TryGetComp<CompApparelRequirement>();
                
                if (reqComp != null)
                {
                    if (reqComp.HasRequiredTrait(pawn) is false)
                    {
                        cantReason = "RR.RequiresTrait".Translate(reqComp.Props.requiredTrait.degreeDatas[0].label);
                        __result = false;
                        return;
                    }
                    if (reqComp.HasRequiredApparel(pawn) is false)
                    {
                        if (reqComp.Props.requiredApparels.Count == 1)
                        {
                            cantReason = "RR.RequiresApparel".Translate(reqComp.Props.requiredApparels[0].label);
                        }
                        else
                        {
                            cantReason = "RR.RequiresApparelsAnyOf".Translate(string.Join(", ", reqComp.Props.requiredApparels.Select(x => x.label)));
                        }
                        __result = false;
                        return;
                    }
                }
            }
        }
    }
}
