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
        }
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOn(() => Takee == null || (!Takee.Downed && Takee.Awake()));
            foreach (Toil item in base.MakeNewToils())
            {
                yield return item;
            }
        }
    }
}