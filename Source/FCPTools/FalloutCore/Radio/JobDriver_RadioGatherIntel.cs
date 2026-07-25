using Verse.AI;

namespace FCP.Core.Radio;

public class JobDriver_RadioGatherIntel : JobDriver
{
    public override bool TryMakePreToilReservations(bool errorOnFailed)
        => pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);

    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDespawnedOrNull(TargetIndex.A);
        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell).FailOnDespawnedNullOrForbidden(TargetIndex.A);

        CompRadio comp = job.GetTarget(TargetIndex.A).Thing.TryGetComp<CompRadio>();

        Toil listen = ToilMaker.MakeToil("MakeNewToils");
        listen.WithProgressBarToilDelay(TargetIndex.A);
        listen.FailOnDespawnedNullOrForbidden(TargetIndex.A);
        listen.defaultCompleteMode = ToilCompleteMode.Delay;
        listen.defaultDuration = comp?.Props.gatherIntelDurationTicks ?? 15000;
        yield return listen;

        yield return Toils_General.Do(() =>
        {
            comp?.Notify_GatherIntelUsed();
            bool success = comp != null && Rand.Chance(comp.Props.gatherIntelSuccessChance) && RadioUtility.TryRevealNearbySite(pawn);
            if (!success)
                Messages.Message("FCP_Radio_IntelNothingFound".Translate(pawn.Named("PAWN")), pawn, MessageTypeDefOf.NeutralEvent);
        });
    }
}
