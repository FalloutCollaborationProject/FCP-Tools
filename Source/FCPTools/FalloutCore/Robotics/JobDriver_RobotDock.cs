using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace FCP.Core.Robotics
{
    public class JobDriver_RobotDock : JobDriver
    {
        private Thing Bench => job.GetTarget(TargetIndex.A).Thing;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(Bench, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull(TargetIndex.A);
            this.FailOn(() => pawn.GetComp<CompRobotUpgrade>()?.PendingBench != Bench);

            yield return Toils_Goto.GotoCell(Bench.OccupiedRect().CenterCell, PathEndMode.OnCell);

            Toil hold = new Toil
            {
                defaultCompleteMode = ToilCompleteMode.Never,
            };
            yield return hold;
        }
    }
}
