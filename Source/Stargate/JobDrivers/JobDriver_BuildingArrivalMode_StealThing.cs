using Verse.AI;

namespace Thek_BuildingArrivalMode
{
    internal class JobDriver_BuildingArrivalMode_StealThing : JobDriver_GotoNoExitCellCheck
    {
        protected Thing Item => job.GetTarget(TargetIndex.B).Thing;

        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch).FailOnSomeonePhysicallyInteracting(TargetIndex.B);
            yield return Toils_Haul.StartCarryThing(TargetIndex.B);
            foreach (Toil superClassToil in base.MakeNewToils())
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
