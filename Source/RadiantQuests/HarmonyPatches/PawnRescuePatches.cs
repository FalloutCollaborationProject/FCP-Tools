using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;

namespace FCP_RadiantQuests
{
    /// <summary>
    /// Patches the targeting parameters to be able to target all prisoners that dont belong to the player
    /// to free them instead of only those that have mindState.WillJoinColonyIfRescued
    /// </summary>
    [HarmonyPatch]
    public static class ReleasePrisonerRightClickPatch
    {
        private static MethodBase TargetMethod()
        {
            return AccessTools.Method(typeof(TargetingParameters), nameof(TargetingParameters.ForQuestPawnsWhoWillJoinColony));
        }
        private static bool Prefix(ref TargetingParameters __result, Pawn p)
        {
            __result = new TargetingParameters
            {
                canTargetPawns = true,
                canTargetBuildings = false,
                mapObjectTargetsMustBeAutoAttackable = false,
                validator = (TargetInfo x) => x.Thing is Pawn { Dead: false } pawn && pawn.IsPrisoner && !pawn.IsPrisonerOfColony
            };
            return false;
        }
    }
    /// <summary>
    /// Patches the job driver to let the pawn run away if it doesnt have mindState.WillJoinColonyIfRescued
    /// </summary>
    [HarmonyPatch]
    public static class JobDriver_OfferHelpPatch
    {
        private static MethodBase TargetMethod()
        {
            return AccessTools.Method(typeof(JobDriver_OfferHelp), "MakeNewToils");
        }
        private static IEnumerable<Toil> Postfix(IEnumerable<Toil> toils, JobDriver_OfferHelp __instance)
        {

            __instance.globalFailConditions.Clear();
            __instance.FailOnDespawnedOrNull(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            yield return Toils_General.DoAtomic(delegate
            {
                Log.Message(__instance.OtherPawn.NameFullColored);
                Log.Message(__instance.pawn.NameFullColored);
                Log.Message(__instance.OtherPawn.mindState.WillJoinColonyIfRescued);
                if (__instance.OtherPawn.mindState.WillJoinColonyIfRescued || PawnRescueUtility.prisonersWillingJoin.Contains(__instance.OtherPawn))
                {
                    Log.Message("Joining colony");
                    InteractionWorker_RecruitAttempt.DoRecruit(__instance.pawn, __instance.OtherPawn, useAudiovisualEffects: false);
                    if (__instance.OtherPawn.needs != null && __instance.OtherPawn.needs.mood != null)
                    {
                        __instance.OtherPawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.Rescued);
                        __instance.OtherPawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.RescuedMeByOfferingHelp, __instance.pawn);
                    }
                    Find.LetterStack.ReceiveLetter("LetterLabelRescueQuestFinished".Translate(), "LetterRescueQuestFinished".Translate(__instance.OtherPawn.Named("PAWN")).AdjustedFor(__instance.OtherPawn).CapitalizeFirst(), LetterDefOf.PositiveEvent, __instance.OtherPawn);
                    PawnRescueUtility.prisonersWillingJoin.Remove(__instance.OtherPawn);
                    QuestUtility.SendQuestTargetSignals(__instance.OtherPawn.questTags, "RescuedFromPrison", __instance.OtherPawn.Named("SUBJECT"));
                }
                else
                {
                    __instance.OtherPawn.guest.SetGuestStatus(null);

                    //__instance.OtherPawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(DefDatabase<JobDef>.GetNamedSilentFail("Job"), __instance.OtherPawn), JobTag.Escaping, false);
                }

            });
        }
    }

}
