using HarmonyLib;
using System.Linq;
using Verse;

namespace WeaponRequirement
{
    [HarmonyPatch(typeof(Pawn_EquipmentTracker), "EquipmentTrackerTick")]
    public static class Pawn_EquipmentTracker_EquipmentTrackerTick_Patch
    {
        public static void Postfix(Pawn_EquipmentTracker __instance)
        {
            foreach (var equipment in __instance.AllEquipmentListForReading.ToList())
            {
                var comp = equipment.GetComp<CompWeaponRequirement>();
                if (comp != null)
                {
                    comp.CompTick();
                }
            }
        }
    }
}
