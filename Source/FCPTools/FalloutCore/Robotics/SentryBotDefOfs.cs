using RimWorld;
using Verse;

namespace FCP.Core.Robotics
{
    [DefOf]
    public static class PawnKindDefOf_SentryBot
    {
        [MayRequire("Rick.FCP.Robotics")]
        public static PawnKindDef FCP_Pawnkind_SentryBot_Minigun;
        [MayRequire("Rick.FCP.Robotics")]
        public static PawnKindDef FCP_Pawnkind_SentryBot_Rocket;

        static PawnKindDefOf_SentryBot()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(PawnKindDefOf_SentryBot));
        }
    }

    [DefOf]
    public static class HediffDefOf_SentryBot
    {
        [MayRequire("Rick.FCP.Robotics")]
        public static HediffDef FCP_Hediff_SentryBot_Minigun;
        [MayRequire("Rick.FCP.Robotics")]
        public static HediffDef FCP_Hediff_SentryBot_Rocket;

        static HediffDefOf_SentryBot()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(HediffDefOf_SentryBot));
        }
    }
}
