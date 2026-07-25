using FCP.Enlist;
using Verse.AI;

namespace FCP.Core.Radio;

public class JobDriver_RadioCallReinforcements : JobDriver
{
    public Faction targetFaction;
    public FactionEnlistOptionsDef enlistOptions;
    public RoyalTitlePermitDef permit;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_References.Look(ref targetFaction, "targetFaction");
        Scribe_Defs.Look(ref enlistOptions, "enlistOptions");
        Scribe_Defs.Look(ref permit, "permit");
    }

    public override bool TryMakePreToilReservations(bool errorOnFailed)
        => pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);

    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDespawnedOrNull(TargetIndex.A);
        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);

        Toil call = ToilMaker.MakeToil("MakeNewToils");
        call.initAction = () =>
        {
            if (targetFaction == null || enlistOptions == null || permit == null)
                return;

            RoyalTitlePermitWorker_CallReinforcements.TryCall(pawn, targetFaction, enlistOptions, permit, job.targetB.Cell);
        };
        call.defaultCompleteMode = ToilCompleteMode.Instant;
        yield return call;
    }
}
