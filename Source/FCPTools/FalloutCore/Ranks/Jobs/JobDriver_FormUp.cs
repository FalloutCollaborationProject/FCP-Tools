using Verse.AI;

namespace FCP.Ranks.Jobs;

public class JobDriver_FormUp : JobDriver
{
    private Pawn Commander => (Pawn)job.GetTarget(TargetIndex.A).Thing;

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return true;
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
        this.FailOnDowned(TargetIndex.A);
        this.FailOnMentalState(TargetIndex.A);

        yield return Toils_Goto.GotoCell(TargetIndex.B, PathEndMode.OnCell);

        Toil hold = ToilMaker.MakeToil("HoldFormation");
        hold.tickAction = delegate
        {
            if (Commander != null && Commander.Spawned && pawn.Rotation != Commander.Rotation)
                pawn.Rotation = Commander.Rotation;
        };
        hold.defaultCompleteMode = ToilCompleteMode.Never;
        yield return hold;
    }
}
