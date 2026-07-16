using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace FCP.Core.Robotics
{
    public class JobDriver_HackRobot : JobDriver
    {
        private Pawn Target => (Pawn)job.GetTarget(TargetIndex.A).Thing;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(Target, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull(TargetIndex.A);
            this.FailOn(() => Target.GetComp<CompHackable>()?.CanBeHacked != true);

            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);

            CompHackable hackable = Target.GetComp<CompHackable>();
            Toil work = Toils_General.Wait(hackable?.Props.workTicks ?? 600);
            work.WithProgressBarToilDelay(TargetIndex.A);
            work.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
            yield return work;

            yield return new Toil
            {
                initAction = () => Target.GetComp<CompHackable>()?.AttemptHack(pawn),
                defaultCompleteMode = ToilCompleteMode.Instant,
            };
        }
    }
}
