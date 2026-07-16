using RimWorld;
using Verse;
using Verse.AI;

namespace FCP.Core.Robotics
{
    public class JobGiver_RobotPoweredDown : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            Job job = JobMaker.MakeJob(JobDefOf.Wait_MaintainPosture);
            job.expiryInterval = -1;
            job.checkOverrideOnExpire = true;
            return job;
        }
    }
}
