using RimWorld;
using Verse;

namespace FCP.Core.Robotics
{
    [DefOf]
    public static class PawnKindDefOf_Securitron
    {
        [MayRequire("Rick.FCP.Robotics")]
        public static PawnKindDef FCP_Pawnkind_Securitron_Normal;
        [MayRequire("Rick.FCP.Robotics")]
        public static PawnKindDef FCP_Pawnkind_Securitron_Police;
        [MayRequire("Rick.FCP.Robotics")]
        public static PawnKindDef FCP_Pawnkind_Securitron_Soldier;
        [MayRequire("Rick.FCP.Robotics")]
        public static PawnKindDef FCP_Pawnkind_Securitron_Berserk;

        static PawnKindDefOf_Securitron()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(PawnKindDefOf_Securitron));
        }
    }

    [DefOf]
    public static class ThingDefOf_Securitron
    {
        [MayRequire("Rick.FCP.Robotics")]
        public static ThingDef FCP_Securitron;

        static ThingDefOf_Securitron()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(ThingDefOf_Securitron));
        }
    }

    [DefOf]
    public static class AbilityDefOf_Securitron
    {
        [MayRequire("Rick.FCP.Robotics")]
        public static AbilityDef FCP_Ability_Securitron_Rockets;

        static AbilityDefOf_Securitron()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(AbilityDefOf_Securitron));
        }
    }

    [DefOf]
    public static class HediffDefOf_Securitron
    {
        [MayRequire("Rick.FCP.Robotics")]
        public static HediffDef FCP_Hediff_Securitron_Face_Empty;
        [MayRequire("Rick.FCP.Robotics")]
        public static HediffDef FCP_Hediff_Securitron_Face_Corrupt;
        [MayRequire("Rick.FCP.Robotics")]
        public static HediffDef FCP_Hediff_Securitron_Face_Army;
        [MayRequire("Rick.FCP.Robotics")]
        public static HediffDef FCP_Hediff_Securitron_Face_Cop;
        [MayRequire("Rick.FCP.Robotics")]
        public static HediffDef FCP_Hediff_Securitron_Face_Female;
        [MayRequire("Rick.FCP.Robotics")]
        public static HediffDef FCP_Hediff_Securitron_Face_Male;
        [MayRequire("Rick.FCP.Robotics")]
        public static HediffDef FCP_Hediff_Securitron_Face_Smily;
        [MayRequire("Rick.FCP.Robotics")]
        public static HediffDef FCP_Hediff_Securitron_Face_Victor;
        [MayRequire("Rick.FCP.Robotics")]
        public static HediffDef FCP_Hediff_Securitron_Gun;
        [MayRequire("Rick.FCP.Robotics")]
        public static HediffDef FCP_Hediff_Securitron_GrenadeLauncher;
        [MayRequire("Rick.FCP.Robotics")]
        public static HediffDef FCP_Hediff_Securitron_RocketPod;

        static HediffDefOf_Securitron()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(HediffDefOf_Securitron));
        }
    }

    [DefOf]
    public static class BodyPartGroupDefOf_Securitron
    {
        [MayRequire("Rick.FCP.Robotics")]
        public static BodyPartGroupDef SecuritronScreen;
        [MayRequire("Rick.FCP.Robotics")]
        public static BodyPartGroupDef SecuritronShoulder;
        [MayRequire("Rick.FCP.Robotics")]
        public static BodyPartGroupDef SecuritronWeaponMount;

        static BodyPartGroupDefOf_Securitron()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(BodyPartGroupDefOf_Securitron));
        }
    }
}
