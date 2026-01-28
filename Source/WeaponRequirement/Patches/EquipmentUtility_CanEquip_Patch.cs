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
        
        var ext = thing.def.GetModExtension<WeaponRequirementExtension>();
        if (ext == null)
            return;

        if (!ext.dontBlockEquip && !ext.RequirementsMet(pawn, thing, onTick: false))
        {
            __result = false;
            cantReason = "TBD requirements not met";
        }
    }
}