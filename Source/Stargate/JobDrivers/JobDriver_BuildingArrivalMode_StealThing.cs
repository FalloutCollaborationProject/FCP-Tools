using Verse.AI;

namespace Thek_BuildingArrivalMode
{
    internal class JobDriver_BuildingArrivalMode_StealThing : JobDriver_GotoNoExitCellCheck
    {
        protected Thing Item => job.GetTarget(TargetIndex.B).Thing;

        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch).FailOnSomeonePhysicallyInteracting(TargetIndex.B);
            // Goes to the thing we want to steal
            yield return Toils_Haul.StartCarryThing(TargetIndex.B);
            // Carries it
            foreach (Toil superClassToil in base.MakeNewToils())
            // Calls for the base's toils, which are the toils inside JobDriver_GotoNoExitCellCheck
            // Makes the pawn go back to the cell they've came from to leave
            {
                yield return superClassToil;
            }
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(Item, job, 1, -1, null, errorOnFailed);
        }
    }
}
