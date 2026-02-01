using System.Reflection;
using HarmonyLib;
using Verse.AI;

namespace FCP.Core;

[HarmonyPatch(typeof(JobGiver_AIFightEnemy), "TryGiveJob")]
public static class JobGiver_AIFightEnemy_TryGiveJob_Patch
{
    private static readonly FieldInfo AbilityField = AccessTools.Field(typeof(JobGiver_AICastAbility), "ability");
    private static readonly MethodInfo TryGiveJobMethod = AccessTools.Method(typeof(JobGiver_AICastAbility), "TryGiveJob");

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
        foreach (var abilityDef in DefDatabase<AbilityDef>.AllDefs.InRandomOrder())
        {
            if (abilityDef.comps != null && abilityDef.comps.OfType<CompProperties_SummonWeapon>().FirstOrDefault() != null)
            {
                var jbg = new JobGiver_AICastSummonWeapon();
                AbilityField.SetValue(jbg, abilityDef);
                var otherJob = (Job)TryGiveJobMethod.Invoke(jbg, [pawn]);
                if (otherJob != null)
                {
                    __result = otherJob;
                    return;
                }
            }
        }
    }
}
