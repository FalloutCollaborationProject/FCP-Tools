using HarmonyLib;
// ReSharper disable InconsistentNaming

namespace FCP.WeaponRequirement;

[HarmonyPatch(typeof(Pawn_EquipmentTracker), "EquipmentTrackerTick")]
public static class Pawn_EquipmentTracker_EquipmentTrackerTick_Patch
{
    public static void Postfix(Pawn_EquipmentTracker __instance)
    {
        // Only Tick this rarely
        if (Find.TickManager.TicksGame % WeaponRequirementUtility.TickInterval != 0)
            return;
        
        foreach (ThingWithComps equipment in __instance.AllEquipmentListForReading)
        {
            var ext = equipment.def.GetModExtension<WeaponRequirementExtension>();
            if (ext == null)
                continue;
            
            WeaponRequirementUtility.EquipmentTrackerTick(ext, __instance.pawn, equipment);
        }
    }
}