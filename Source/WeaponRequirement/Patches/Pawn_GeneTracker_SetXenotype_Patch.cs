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
            if (!equipment.TryGetComp(out CompWeaponRequirement comp))
                continue;

            HediffDef hediffDef = comp.Props.requirementsNotMetHediff;
            Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(hediffDef);

            if (comp.RequirementsMet(pawn))
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
