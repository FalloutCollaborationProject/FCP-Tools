using RimWorld;
using Verse;
using Verse.AI;

namespace FCP.Core.Robotics
{
    public class JobGiver_RobotPoweredDown : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            Building_Bed bed = RobotUtility.FindAssignedBed(pawn);
            if (bed != null && pawn.Position != bed.OccupiedRect().CenterCell)
            {
                Job dockJob = JobMaker.MakeJob(JobDefOf_Robotics.FCP_RobotPowerDown, bed);
                dockJob.expiryInterval = -1;
                dockJob.checkOverrideOnExpire = true;
                return dockJob;
            }

            Job job = JobMaker.MakeJob(JobDefOf.Wait_MaintainPosture);
            job.expiryInterval = -1;
            job.checkOverrideOnExpire = true;
            return job;
        }
    }
}
