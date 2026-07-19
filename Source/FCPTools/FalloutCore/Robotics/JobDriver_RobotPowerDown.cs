using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace FCP.Core.Robotics
{
    public class JobDriver_RobotPowerDown : JobDriver
    {
        private Thing Bed => job.GetTarget(TargetIndex.A).Thing;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return Bed == null || pawn.Reserve(Bed, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            if (Bed != null)
            {
                this.FailOnDestroyedOrNull(TargetIndex.A);
                yield return Toils_Goto.GotoCell(Bed.OccupiedRect().CenterCell, PathEndMode.OnCell);
            }

            Toil hold = new Toil
            {
                defaultCompleteMode = ToilCompleteMode.Never,
            };
            yield return hold;
        }
    }
}
