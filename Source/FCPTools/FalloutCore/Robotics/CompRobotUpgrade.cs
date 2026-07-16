using Verse;

namespace FCP.Core.Robotics
{
    public class CompProperties_RobotUpgrade : CompProperties
    {
        public CompProperties_RobotUpgrade()
        {
            compClass = typeof(CompRobotUpgrade);
        }
    }

    public class CompRobotUpgrade : ThingComp
    {
        public Thing PendingBench;

        public CompProperties_RobotUpgrade Props => (CompProperties_RobotUpgrade)props;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_References.Look(ref PendingBench, "pendingUpgradeBench");
        }
    }
}
