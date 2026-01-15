using HarmonyLib;
using System.Linq;
using Verse;

namespace BM_WeaponSummon
{
    [HarmonyPatch(typeof(Pawn_EquipmentTracker), "EquipmentTrackerTick")]
    public static class Pawn_EquipmentTracker_EquipmentTrackerTick_Patch
    {
        public static void Postfix(Pawn_EquipmentTracker __instance)
        {
            foreach (var equipment in __instance.AllEquipmentListForReading.ToList())
            {
                var comp = equipment.GetComp<CompSummonedWeapon>();
                if (comp != null)
                {
                    comp.CompTick();
                }
            }
        }
    }
}
