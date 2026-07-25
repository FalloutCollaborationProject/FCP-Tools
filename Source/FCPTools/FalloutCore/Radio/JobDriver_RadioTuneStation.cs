using Verse.AI;

namespace FCP.Core.Radio;

public class JobDriver_RadioTuneStation : JobDriver
{
    public override bool TryMakePreToilReservations(bool errorOnFailed)
        => pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);

    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDespawnedOrNull(TargetIndex.A);
        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);

        Toil tune = ToilMaker.MakeToil("MakeNewToils");
        tune.initAction = () =>
        {
            CompRadio comp = job.GetTarget(TargetIndex.A).Thing?.TryGetComp<CompRadio>();
            if (comp != null)
                Find.WindowStack.Add(new Dialog_RadioStations(comp.Props.stations));
        };
        tune.defaultCompleteMode = ToilCompleteMode.Instant;
        yield return tune;
    }
}
