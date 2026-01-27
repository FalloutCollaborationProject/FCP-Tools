using HarmonyLib;
// ReSharper disable InconsistentNaming

namespace FCP.WeaponRequirement;

[HarmonyPatch(typeof(Pawn_EquipmentTracker), "EquipmentTrackerTick")]
public static class Pawn_EquipmentTracker_EquipmentTrackerTick_Patch
{
    public static void Postfix(Pawn_EquipmentTracker __instance)
    {
        // Only Tick this rarely
        if (Find.TickManager.TicksGame % CompWeaponRequirement.TickInterval != 0)
            return;
        
        foreach (ThingWithComps equipment in __instance.AllEquipmentListForReading)
        {
            if (equipment.TryGetComp(out CompWeaponRequirement comp))
            {
                comp.EquipmentTrackerTick();
            }
        }
    }
}