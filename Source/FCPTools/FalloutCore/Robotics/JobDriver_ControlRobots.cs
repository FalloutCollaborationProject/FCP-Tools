using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace FCP.Core.Robotics
{
    public class JobDriver_ControlRobots : JobDriver
    {
        private Thing Terminal => job.GetTarget(TargetIndex.A).Thing;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(Terminal, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull(TargetIndex.A);

            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);

            yield return new Toil
            {
                initAction = () => Terminal.TryGetComp<CompRobotController>()?.OpenRobotSelectMenu(),
                defaultCompleteMode = ToilCompleteMode.Instant,
            };
        }
    }
}
