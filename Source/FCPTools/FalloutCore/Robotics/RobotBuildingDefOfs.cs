using RimWorld;
using Verse;

namespace FCP.Core.Robotics
{
    [DefOf]
    public static class ThingDefOf_Robotics
    {
        [MayRequire("Rick.FCP.Robotics")]
        public static ThingDef FCP_Building_RobotBed;

        static ThingDefOf_Robotics()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(ThingDefOf_Robotics));
        }
    }

    [DefOf]
    public static class ThingDefOf_SentryBot
    {
        [MayRequire("Rick.FCP.Robotics")]
        public static ThingDef FCP_SentryBot;

        static ThingDefOf_SentryBot()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(ThingDefOf_SentryBot));
        }
    }
}
