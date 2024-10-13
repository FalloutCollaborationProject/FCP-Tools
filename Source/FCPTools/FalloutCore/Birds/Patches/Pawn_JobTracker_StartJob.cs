using HarmonyLib;
using Verse.AI;

namespace FCP.Core.Birds;

[HarmonyPatchCategory("Birds")]
[HarmonyPatch(typeof(Pawn_JobTracker), nameof(Pawn_JobTracker.StartJob))]
public class Pawn_JobTracker_StartJob
{
    private static void Postfix(Pawn ___pawn)
    {
        if (!___pawn.IsFlyingPawn(out var comp))
        {
            return;
        }

        var curJob = ___pawn.CurJob;
        if (curJob == null)
        {
            return;
        }

        if ((!comp.Props.flyWhenFleeing || curJob.def != JobDefOf.Flee && curJob.def != JobDefOf.FleeAndCower)
            && (curJob.def != JobDefOf.GotoWander || !Rand.Chance(comp.Props.flyWhenWanderingChance))
            && (curJob.def != JobDefOf.PredatorHunt || !comp.Props.flyWhenHunting))
        {
            return;
        }

        comp.isFlyingCurrently = true;
        curJob.locomotionUrgency = LocomotionUrgency.Jog;
        comp.ChangeGraphic(true);
    }
}