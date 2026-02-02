using HarmonyLib;

namespace FCP.Core.PowerArmor;

[HarmonyPatch(typeof(ThingWithComps), "GetFloatMenuOptions")]
public static class ThingWithComps_GetFloatMenuOptions_Patch
{
    public static IEnumerable<FloatMenuOption> Postfix(IEnumerable<FloatMenuOption> __result, ThingWithComps __instance,
        Pawn selPawn)
    {
        foreach (FloatMenuOption option in __result)
        {
            yield return option;
        }

        if (__instance is not Pawn pawn || !pawn.RaceProps.Humanlike || pawn.Faction != selPawn.Faction) 
            yield break;
        
        foreach (Apparel apparel in pawn.apparel.WornApparel)
        {
            var comp = apparel.GetComp<CompPowerArmor>();
            if (comp == null) continue;
            
            if (JobGiver_Reload_TryGiveJob_Patch.CanRefuel(selPawn, apparel, forced: true))
            {
                var job = JobGiver_Reload_TryGiveJob_Patch.RefuelJob(pawn, apparel, pawn);
                var scanner = PowerArmorDefOf.Refuel.Worker as WorkGiver_Scanner;
                yield return new FloatMenuOption("PrioritizeGeneric".Translate(scanner.PostProcessedGerund(job), apparel.Label).CapitalizeFirst(), delegate
                {
                    selPawn.jobs.TryTakeOrderedJob(job);
                });
            }
        }
    }
}