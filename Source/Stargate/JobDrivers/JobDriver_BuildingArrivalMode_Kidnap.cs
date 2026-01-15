using Verse.AI;

namespace Thek_BuildingArrivalMode
{
    internal class JobDriver_BuildingArrivalMode_Kidnap : JobDriver_BuildingArrivalMode_StealThing
    {
        protected Pawn Takee => (Pawn)job.GetTarget(TargetIndex.B);
        public override string GetReport()
        {
            if (Item == null || pawn.HostileTo(Takee))
            {
                return base.GetReport();
            }
            return JobUtility.GetResolvedJobReport(JobDefOfs.Thek_BuildingArrivalMode_Kidnap.reportString, Takee);
            // This is the message that appears in the window when selecting a pawn me thinks, i copied it from JobDriver_Kidnap
        }
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOn(() => Takee == null || (!Takee.Downed && Takee.Awake()));
            foreach (Toil item in base.MakeNewToils())
            // Calls the toils inside the superclass, so in JobDriver_BuildingArrivalMode_StealThing
            // This class just handles changing the target to a pawn and writing it on the pawn's window tbf
            // The superclass handles going to and grabbing it
            {
                yield return item;
            }
        }
    }
}