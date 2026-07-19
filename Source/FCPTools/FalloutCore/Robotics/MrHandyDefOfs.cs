using RimWorld;
using Verse;

namespace FCP.Core.Robotics
{
    [DefOf]
    public static class PawnKindDefOf_MrHandy
    {
        [MayRequire("Rick.FCP.Robotics")]
        public static PawnKindDef FCP_Pawnkind_Mr_Handy;
        [MayRequire("Rick.FCP.Robotics")]
        public static PawnKindDef FCP_Pawnkind_Ms_Nanny;
        [MayRequire("Rick.FCP.Robotics")]
        public static PawnKindDef FCP_Pawnkind_Mr_Gutsy;
        [MayRequire("Rick.FCP.Robotics")]
        public static PawnKindDef FCP_Pawnkind_Mr_Orderly;

        static PawnKindDefOf_MrHandy()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(PawnKindDefOf_MrHandy));
        }
    }

    [DefOf]
    public static class ThingDefOf_MrHandy
    {
        [MayRequire("Rick.FCP.Robotics")]
        public static ThingDef FCP_MrHandy;

        static ThingDefOf_MrHandy()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(ThingDefOf_MrHandy));
        }
    }

    [DefOf]
    public static class HediffDefOf_MrHandy
    {
        [MayRequire("Rick.FCP.Robotics")]
        public static HediffDef FCP_Hediff_MrHandy_ArmStalk_Left;
        [MayRequire("Rick.FCP.Robotics")]
        public static HediffDef FCP_Hediff_MrHandy_ArmStalk_Center;
        [MayRequire("Rick.FCP.Robotics")]
        public static HediffDef FCP_Hediff_MrHandy_ArmStalk_Right;
        [MayRequire("Rick.FCP.Robotics")]
        public static HediffDef FCP_Hediff_MrHandy_Left_Saw;
        [MayRequire("Rick.FCP.Robotics")]
        public static HediffDef FCP_Hediff_MrHandy_Left_Pincer;
        [MayRequire("Rick.FCP.Robotics")]
        public static HediffDef FCP_Hediff_MrHandy_Center_Pincer;
        [MayRequire("Rick.FCP.Robotics")]
        public static HediffDef FCP_Hediff_MrHandy_Right_Flamer;
        [MayRequire("Rick.FCP.Robotics")]
        public static HediffDef FCP_Hediff_MrHandy_Right_Pincer;
        [MayRequire("Rick.FCP.Robotics")]
        public static HediffDef FCP_Hediff_MrHandy_Gun;
        [MayRequire("Rick.FCP.Robotics")]
        public static HediffDef FCP_Hediff_MrHandy_Laser;
        [MayRequire("Rick.FCP.Robotics")]
        public static HediffDef FCP_Hediff_MrHandy_Plasma;
        [MayRequire("Rick.FCP.Robotics")]
        public static HediffDef FCP_Hediff_MrHandy_Thruster;
        [MayRequire("Rick.FCP.Robotics")]
        public static HediffDef FCP_Hediff_MrHandy_Eye_Left;
        [MayRequire("Rick.FCP.Robotics")]
        public static HediffDef FCP_Hediff_MrHandy_Eye_Center;
        [MayRequire("Rick.FCP.Robotics")]
        public static HediffDef FCP_Hediff_MrHandy_Eye_Right;
        [MayRequire("Rick.FCP.Robotics")]
        public static HediffDef FCP_Hediff_MrHandy_Eye_Left_Armored;
        [MayRequire("Rick.FCP.Robotics")]
        public static HediffDef FCP_Hediff_MrHandy_Eye_Center_Armored;
        [MayRequire("Rick.FCP.Robotics")]
        public static HediffDef FCP_Hediff_MrHandy_Eye_Right_Armored;
        [MayRequire("Rick.FCP.Robotics")]
        public static HediffDef FCP_Hediff_MrHandy_BodyArmored;

        static HediffDefOf_MrHandy()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(HediffDefOf_MrHandy));
        }
    }

    [DefOf]
    public static class BodyPartGroupDefOf_MrHandy
    {
        [MayRequire("Rick.FCP.Robotics")]
        public static BodyPartGroupDef MrHandyArmLeft;
        [MayRequire("Rick.FCP.Robotics")]
        public static BodyPartGroupDef MrHandyArmCenter;
        [MayRequire("Rick.FCP.Robotics")]
        public static BodyPartGroupDef MrHandyArmRight;
        [MayRequire("Rick.FCP.Robotics")]
        public static BodyPartGroupDef MrHandyEyeLeft;
        [MayRequire("Rick.FCP.Robotics")]
        public static BodyPartGroupDef MrHandyEyeCenter;
        [MayRequire("Rick.FCP.Robotics")]
        public static BodyPartGroupDef MrHandyEyeRight;
        [MayRequire("Rick.FCP.Robotics")]
        public static BodyPartGroupDef MrHandyThruster;

        static BodyPartGroupDefOf_MrHandy()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(BodyPartGroupDefOf_MrHandy));
        }
    }
}
