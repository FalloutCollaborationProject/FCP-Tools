using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace FCP.Core
{
    [HarmonyPatch(typeof(CompTechprint), nameof(CompTechprint.CompFloatMenuOptions))]
    public static class CompTechprint_CompFloatMenuOptions_Patch
    {
        public static IEnumerable<FloatMenuOption> Postfix(IEnumerable<FloatMenuOption> __result, 
        CompTechprint __instance, Pawn selPawn)
        {
            var techprintCompProps = __instance.Props;
            if (techprintCompProps != null && techprintCompProps.project != null)
            {
                var extension = techprintCompProps.project.GetModExtension<TechprintExtension>();
                if (extension != null && extension.requiredBench != null)
                {
                    foreach (var item in CompFloatMenuOptions(__instance, selPawn, extension.requiredBench))
                    {
                        yield return item;
                    }
                    yield break;
                }
            }
            
            foreach (var item in __result)
            {
                yield return item;
            }
        }


        public static IEnumerable<FloatMenuOption> CompFloatMenuOptions(CompTechprint __instance, 
        Pawn selPawn, ThingDef requiredBench)
        {
            var parent = __instance.parent;
            if (!ModLister.CheckRoyalty("Techprint"))
            {
                yield break;
            }
            JobFailReason.Clear();
            if (selPawn.WorkTypeIsDisabled(WorkTypeDefOf.Research) || selPawn.WorkTagIsDisabled(WorkTags.Intellectual))
            {
                JobFailReason.Is("WillNever".Translate("Research".TranslateSimple().UncapitalizeFirst()));
            }
            else if (!selPawn.CanReach(parent, PathEndMode.ClosestTouch, Danger.Some))
            {
                JobFailReason.Is("CannotReach".Translate());
            }
            else if (!selPawn.CanReserve(parent))
            {
                Pawn pawn = selPawn.Map.reservationManager.FirstRespectedReserver(parent, selPawn);
                if (pawn == null)
                {
                    pawn = selPawn.Map.physicalInteractionReservationManager.FirstReserverOf(selPawn);
                }
                if (pawn != null)
                {
                    JobFailReason.Is("ReservedBy".Translate(pawn.LabelShort, pawn));
                }
                else
                {
                    JobFailReason.Is("Reserved".Translate());
                }
            }
            HaulAIUtility.PawnCanAutomaticallyHaul(selPawn, parent, forced: true);
            Thing thing2 = GenClosest.ClosestThingReachable(selPawn.Position, selPawn.Map,
             ThingRequest.ForDef(requiredBench), PathEndMode.InteractionCell, TraverseParms.For(selPawn, Danger.Some), 9999f, (Thing thing) => thing is Building_ResearchBench && !thing.IsForbidden(selPawn) && selPawn.CanReserve(thing));
            Job job = null;
            if (thing2 != null)
            {
                job = JobMaker.MakeJob(JobDefOf.ApplyTechprint);
                job.targetA = thing2;
                job.targetB = parent;
                job.targetC = thing2.Position;
            }
            if (JobFailReason.HaveReason)
            {
                yield return new FloatMenuOption("CannotGenericWorkCustom".Translate("ApplyTechprint".Translate(parent.Label)) + ": " + JobFailReason.Reason.CapitalizeFirst(), null);
                JobFailReason.Clear();
                yield break;
            }
            yield return new FloatMenuOption("ApplyTechprint".Translate(parent.Label).CapitalizeFirst(), delegate
            {
                if (job == null)
                {
                    Messages.Message("MessageNoResearchBenchForTechprint".Translate(), MessageTypeDefOf.RejectInput);
                }
                else
                {
                    selPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                }
            });
        }
    }
}