using RimWorld;
using Verse;

namespace FCP.Core.Robotics
{
    [DefOf]
    public static class PawnKindDefOf_Assaultron
    {
        [MayRequire("Rick.FCP.Robotics")]
        public static PawnKindDef FCP_Pawnkind_Assaultron;

        static PawnKindDefOf_Assaultron()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(PawnKindDefOf_Assaultron));
        }
    }

    [DefOf]
    public static class ThingDefOf_Assaultron
    {
        [MayRequire("Rick.FCP.Robotics")]
        public static ThingDef FCP_Assaultron;

        static ThingDefOf_Assaultron()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(ThingDefOf_Assaultron));
        }
    }

    [DefOf]
    public static class HediffDefOf_Assaultron
    {
        [MayRequire("Rick.FCP.Robotics")]
        public static HediffDef FCP_Hediff_Assaultron_Hand_Claw;
        [MayRequire("Rick.FCP.Robotics")]
        public static HediffDef FCP_Hediff_Assaultron_Hand_Blade;
        [MayRequire("Rick.FCP.Robotics")]
        public static HediffDef FCP_Hediff_Assaultron_BodyType_Army;

        static HediffDefOf_Assaultron()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(HediffDefOf_Assaultron));
        }
    }

    [DefOf]
    public static class BodyPartGroupDefOf_Assaultron
    {
        [MayRequire("Rick.FCP.Robotics")]
        public static BodyPartGroupDef AssaultronHands;

        static BodyPartGroupDefOf_Assaultron()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(BodyPartGroupDefOf_Assaultron));
        }
    }
}
