using HarmonyLib;
// ReSharper disable InconsistentNaming

namespace FCP.WeaponRequirement;

[HarmonyPatch(typeof(Pawn_GeneTracker))]
public static class Pawn_GeneTracker_SetXenotype_Patch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(Pawn_GeneTracker.SetXenotype))]
    [HarmonyPatch(nameof(Pawn_GeneTracker.SetXenotypeDirect))]
    public static void Postfix(Pawn_GeneTracker __instance)
    {
        Pawn pawn = __instance.pawn;

        if (pawn?.equipment == null)
            return;

        foreach (ThingWithComps equipment in pawn.equipment.AllEquipmentListForReading)
        {
            var ext = equipment.def.GetModExtension<WeaponRequirementExtension>();
            if (ext == null)
                continue;

            HediffDef hediffDef = ext.requirementsNotMetHediff;
            Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(hediffDef);

            if (ext.RequirementsMet(pawn, equipment, onTick: false))
            {
                if (hediff != null)
                    pawn.health.RemoveHediff(hediff);
            }
            else if (hediff == null)
            {
                pawn.health.AddHediff(hediffDef);
            }
        }
    }
}
