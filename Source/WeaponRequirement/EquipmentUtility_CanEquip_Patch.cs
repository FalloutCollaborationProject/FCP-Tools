using HarmonyLib;
using RimWorld;
using System.Linq;
using Verse;

namespace WeaponRequirement
{
    [HarmonyPatch(typeof(EquipmentUtility), "CanEquip", [typeof(Thing), typeof(Pawn), typeof(string), typeof(bool)],
    [ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Out, ArgumentType.Normal])]
    public static class EquipmentUtility_CanEquip_Patch
    {
        private static void Postfix(ref bool __result, Thing thing, Pawn pawn, ref string cantReason, bool checkBonded = true)
        {
            if (pawn.apparel != null)
            {
                var comp = thing.TryGetComp<CompWeaponRequirement>();
                if (comp != null)
                {
                    if (comp.HasRequiredApparel(pawn) is false)
                    {
                        if (comp.Props.requiredApparels.Count == 1)
                        {
                            cantReason = "BM.RequiresApparel".Translate(comp.Props.requiredApparels[0].label);
                        }
                        else
                        {
                            cantReason = "BM.RequiresApparelsAnyOf".Translate(string.Join(", ", comp.Props.requiredApparels.Select(x => x.label)));
                        }
                        __result = false;
                        return;
                    }
                }
            }
        }
    }
}
