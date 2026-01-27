using HarmonyLib;

namespace FCP.WeaponRequirement;

[HarmonyPatch(typeof(EquipmentUtility), "CanEquip", [typeof(Thing), typeof(Pawn), typeof(string), typeof(bool)],
    [ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Out, ArgumentType.Normal])]
public static class EquipmentUtility_CanEquip_Patch
{
    private static void Postfix(ref bool __result, Thing thing, Pawn pawn, ref string cantReason, bool checkBonded = true)
    {
        if (pawn.apparel == null)
            return;
        
        if (!thing.TryGetComp(out CompWeaponRequirement comp))
            return;

        if (!comp.Props.dontBlockEquip && !comp.RequirementsMet(pawn))
        {
            __result = false;
            cantReason = "TBD requirements not met";
        }
    }
}