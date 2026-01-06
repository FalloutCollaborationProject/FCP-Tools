using HarmonyLib;

namespace RangerRick_PowerArmor
{
    [HarmonyPatch(typeof(ThingWithComps), "GetFloatMenuOptions")]
    public static class ThingWithComps_GetFloatMenuOptions_Patch
    {
        public static IEnumerable<FloatMenuOption> Postfix(IEnumerable<FloatMenuOption> __result, ThingWithComps __instance,
            Pawn selPawn)
        {
            foreach (var floatOption in __result)
            {
                yield return floatOption;
            }
            if (__instance is Pawn pawn && pawn.RaceProps.Humanlike && pawn.Faction == selPawn.Faction)
            {
                foreach (var apparel in pawn.apparel.WornApparel)
                {
                    var comp = apparel.GetComp<CompPowerArmor>();
                    if (comp != null)
                    {
                        if (JobGiver_Reload_TryGiveJob_Patch.CanRefuel(selPawn, apparel, forced: true))
                        {
                            var job = JobGiver_Reload_TryGiveJob_Patch.RefuelJob(pawn, apparel, pawn);
                            var scanner = RR_DefOf.Refuel.Worker as WorkGiver_Scanner;
                            yield return new FloatMenuOption("PrioritizeGeneric".Translate(scanner.PostProcessedGerund(job), apparel.Label).CapitalizeFirst(), delegate
                            {
                                selPawn.jobs.TryTakeOrderedJob(job);
                            });
                        }
                    }
                }
            }
        }
    }
}
