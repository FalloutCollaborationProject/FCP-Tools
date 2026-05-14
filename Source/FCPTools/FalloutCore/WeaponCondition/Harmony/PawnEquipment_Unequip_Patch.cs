using HarmonyLib;

namespace FCP.Core.WeaponCondition;

[HarmonyPatch(typeof(Pawn_EquipmentTracker), nameof(Pawn_EquipmentTracker.TryDropEquipment))]
public static class PawnEquipment_TryDropEquipment_Patch
{
    public static void Prefix(ThingWithComps eq)
    {
        if (!FCPCoreMod.Settings.General.weaponConditionEnabled)
            return;
        eq?.GetComp<CompWeaponCondition>()?.ClearJam();
    }
}

[HarmonyPatch(typeof(Pawn_EquipmentTracker), nameof(Pawn_EquipmentTracker.DestroyEquipment))]
public static class PawnEquipment_DestroyEquipment_Patch
{
    public static void Prefix(ThingWithComps eq)
    {
        if (!FCPCoreMod.Settings.General.weaponConditionEnabled)
            return;
        eq?.GetComp<CompWeaponCondition>()?.ClearJam();
    }
}
