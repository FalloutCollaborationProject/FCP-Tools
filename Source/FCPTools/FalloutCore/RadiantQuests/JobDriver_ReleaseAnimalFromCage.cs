using Verse.AI;

namespace FCP.Core.RadiantQuests;

public class JobDriver_ReleaseAnimalFromCage : JobDriver
{
    private const TargetIndex CasketInd = TargetIndex.A;

    public CompAnimalCage Cage => job.targetA.Thing.TryGetComp<CompAnimalCage>();

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return pawn.Reserve(Cage.parent, job, 1, -1, null, errorOnFailed);
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDestroyedOrNull(TargetIndex.A);
        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
        yield return PrepareToReleaseToil(TargetIndex.A);
        Toil enter = ToilMaker.MakeToil("MakeNewToils");
        enter.initAction = delegate
        {
            if (GameComponent_PawnRescue.Instance.IsWillingToJoin(Cage.Occupant))
            {
                InteractionWorker_RecruitAttempt.DoRecruit(pawn, Cage.Occupant, useAudiovisualEffects: false);
                GameComponent_PawnRescue.Instance.ClearWillingToJoin(Cage.Occupant);
            }
            Cage.EjectContents(Map);
        };
        enter.defaultCompleteMode = ToilCompleteMode.Instant;

        yield return enter;
    }

    public static Toil PrepareToReleaseToil(TargetIndex CageIndex)
    {
        Toil toil = Toils_General.Wait(70);
        toil.FailOnCannotTouch(CageIndex, PathEndMode.InteractionCell);
        toil.WithProgressBarToilDelay(CageIndex);
        return toil;
    }
}