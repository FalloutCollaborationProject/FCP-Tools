using Verse;

namespace FCP.Core.Robotics
{
    [DefOf]
    public static class JobDefOf_Robotics
    {
        public static JobDef FCP_RobotDock;
        public static JobDef FCP_UpgradeRobot;
        public static JobDef FCP_ControlRobots;
        public static JobDef FCP_HackRobot;

        static JobDefOf_Robotics()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(JobDefOf_Robotics));
        }
    }
}
