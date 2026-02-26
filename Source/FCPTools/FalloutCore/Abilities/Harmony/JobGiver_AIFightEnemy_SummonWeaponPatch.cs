using HarmonyLib;
using Verse.AI;

namespace FCP.Core;

[HarmonyPatch(typeof(JobGiver_AIFightEnemy), "TryGiveJob")]
public static class JobGiver_AIFightEnemy_SummonWeaponPatch
{
    private static readonly AccessTools.FieldRef<JobGiver_AICastAbility, AbilityDef> AbilityRef =
        AccessTools.FieldRefAccess<JobGiver_AICastAbility, AbilityDef>("ability");
    
    private static readonly Func<JobGiver_AICastAbility, Pawn, Job> TryGiveJob =
        AccessTools.MethodDelegate<Func<JobGiver_AICastAbility, Pawn, Job>>(
            AccessTools.Method(typeof(JobGiver_AICastAbility), "TryGiveJob"));

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
                AbilityRef(jbg) = abilityDef;
                var otherJob = TryGiveJob(jbg, pawn);
                if (otherJob != null)
                {
                    __result = otherJob;
                    return;
                }
            }
        }
    }
}
