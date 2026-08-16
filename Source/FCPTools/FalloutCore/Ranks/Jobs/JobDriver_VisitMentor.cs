using Verse.AI;

namespace FCP.Ranks.Jobs;

public class JobDriver_VisitMentor : JobDriver
{
    private const float ObservationRateFactor = 0.15f;

    private Pawn Mentor => (Pawn)job.GetTarget(TargetIndex.A).Thing;

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return pawn.Reserve(Mentor, job, 1, -1, null, errorOnFailed);
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
        this.FailOnDowned(TargetIndex.A);
        this.FailOnMentalState(TargetIndex.A);

        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);

        Toil observe = ToilMaker.MakeToil("VisitMentor");
        observe.tickIntervalAction = delegate(int delta)
        {
            PawnRankData data = Current.Game.GetComponent<GameComponent_PawnRanks>()?.GetRankData(pawn);
            List<SkillGain> gains = data?.currentTier?.trainingXpPerHour;
            if (gains != null)
            {
                foreach (SkillGain gain in gains)
                    pawn.skills.Learn(gain.skill, gain.amount * ObservationRateFactor / GenDate.TicksPerHour * delta);
            }

            pawn.rotationTracker.FaceCell(Mentor.Position);
            JoyUtility.JoyTickCheckEnd(pawn, delta);
        };
        observe.handlingFacing = true;
        observe.socialMode = RandomSocialMode.Off;
        observe.defaultCompleteMode = ToilCompleteMode.Delay;
        observe.defaultDuration = job.def.joyDuration;
        yield return observe;

        yield return Toils_General.Do(delegate
        {
            pawn.needs?.mood?.thoughts.memories.TryGainMemory(RankThoughtDefOf.FCP_Thought_ObservedMentor);
        });
    }
}
