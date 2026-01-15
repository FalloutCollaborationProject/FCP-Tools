using HarmonyLib;
using RimWorld;
using System.Linq;
using Verse;
using Verse.AI;

namespace BM_WeaponSummon
{
    [HarmonyPatch(typeof(JobGiver_AIFightEnemy), "TryGiveJob")]
    public static class JobGiver_AIFightEnemy_TryGiveJob_Patch
    {
        public static void Postfix(ref Job __result, Pawn pawn)
        {
            if (__result != null)
            {
                TrySwapToWeaponSummon(ref __result, pawn);
            }
        }

        private static void TrySwapToWeaponSummon(ref Job __result, Pawn pawn)
        {
            var existingWeapon = pawn.equipment?.Primary;
            if (existingWeapon != null && existingWeapon.GetComp<CompSummonedWeapon>() != null)
            {
                return;
            }
            foreach (var ability in DefDatabase<AbilityDef>.AllDefs.InRandomOrder())
            {
                if (ability.comps != null && ability.comps.OfType<CompProperties_SummonWeapon>().FirstOrDefault() != null)
                {
                    var jbg = new JobGiver_AICastSummonWeapon
                    {
                        ability = ability,
                    };
                    var otherJob = jbg.TryGiveJob(pawn);
                    if (otherJob != null)
                    {
                        __result = otherJob;
                        return;
                    }
                }
            }
        }
    }
}
