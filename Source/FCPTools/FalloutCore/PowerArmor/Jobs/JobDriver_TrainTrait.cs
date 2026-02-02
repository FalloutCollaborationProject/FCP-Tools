using Verse.AI;

namespace FCP.Core.PowerArmor;

public class JobDriver_TrainTrait : JobDriver
{
    private int duration;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref duration, "duration", 0);
    }

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return pawn.Reserve(TargetA, job, 1, -1, null, errorOnFailed);
    }

    public override void Notify_Starting()
    {
        base.Notify_Starting();
        duration = TargetA.Thing.TryGetComp<CompTrainer>().Props.trainDuration;
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnBurningImmobile(TargetIndex.A);
        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell).FailOnDespawnedNullOrForbidden(TargetIndex.A);
        Toil toil = ToilMaker.MakeToil("MakeNewToils");
        toil.WithProgressBarToilDelay(TargetIndex.A);
        toil.FailOnDespawnedNullOrForbidden(TargetIndex.A);
        toil.defaultCompleteMode = ToilCompleteMode.Delay;
        toil.defaultDuration = duration;
        yield return toil;
        yield return Toils_General.Do(delegate
        {
            pawn.story.traits.GainTrait(new Trait(TargetA.Thing.TryGetComp<CompTrainer>().Props.givesTrait));
        });
    }
}