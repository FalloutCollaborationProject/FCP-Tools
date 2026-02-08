using FCP.Core.Stims;
using HarmonyLib;
using Verse.AI;

namespace FCP.Core;

[HarmonyPatch(typeof(Pawn), nameof(Pawn.PostApplyDamage))]
public static class Pawn_PostApplyDamage_Patch
{
    [HarmonyPostfix]
    public static void Postfix(Pawn __instance, float totalDamageDealt)
    {
        if (!FCPCoreMod.Settings.General.autoStim)
            return;
        if (__instance.story == null)
            return;
        if (!FCPCoreMod.Settings.General.teetotalerAutoStim &&
            __instance.story.traits.DegreeOfTrait(TraitDefOf.DrugDesire) < 0)
            return;
        if (totalDamageDealt > 0)
        {
            var stimpack = __instance.inventory.innerContainer
                .FirstOrDefault(x => x.def.HasModExtension<ModExtension_IngestibleStim>());
            if (stimpack != null)
            {
                foreach (var hediff in __instance.health.hediffSet.hediffs)
                {
                    if (hediff.TryGetComp<HediffComp_Stimpack>() != null)
                    {
                        return;
                    }
                }
                Job job = JobMaker.MakeJob(JobDefOf.Ingest, stimpack);
                job.count = 1;
                __instance.jobs.TryTakeOrderedJob(job);
            }
        }
    }
}
