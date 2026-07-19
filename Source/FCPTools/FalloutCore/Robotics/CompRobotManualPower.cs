using Verse;
using Verse.AI;

namespace FCP.Core.Robotics
{
    public class CompProperties_RobotManualPower : CompProperties
    {
        public CompProperties_RobotManualPower()
        {
            compClass = typeof(CompRobotManualPower);
        }
    }

    public class CompRobotManualPower : ThingComp
    {
        private bool poweredOn = true;

        public bool PoweredOn => poweredOn;

        public void TogglePower()
        {
            poweredOn = !poweredOn;
            if (parent is Pawn pawn)
            {
                if (pawn.jobs?.curJob != null)
                {
                    pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
                }
                pawn.GetComp<CompSecuritronFace>()?.RefreshFace();
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref poweredOn, "poweredOn", true);
        }
    }
}
