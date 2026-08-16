using Verse.AI;

namespace FCP.Ranks.Jobs;

public class JobDriver_RankSessionPartner : JobDriver
{
    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return true;
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDespawnedNullOrForbidden(TargetIndex.A);

        yield return Toils_General.Wait(job.expiryInterval > 0 ? job.expiryInterval : 5000, TargetIndex.A);
    }
}
