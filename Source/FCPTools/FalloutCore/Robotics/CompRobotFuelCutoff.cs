using Verse;
using Verse.AI;

namespace FCP.Core.Robotics
{
    public class CompProperties_RobotFuelCutoff : CompProperties
    {
        public CompProperties_RobotFuelCutoff()
        {
            compClass = typeof(CompRobotFuelCutoff);
        }
    }

    public class CompRobotFuelCutoff : ThingComp
    {
        public override void ReceiveCompSignal(string signal)
        {
            base.ReceiveCompSignal(signal);
            if (signal != "RanOutOfFuel" && signal != "Refueled")
            {
                return;
            }

            if (parent is Pawn pawn && pawn.jobs?.curJob != null)
            {
                pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
            }
        }
    }
}
