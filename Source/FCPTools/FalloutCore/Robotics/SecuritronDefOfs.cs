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
    public static class ThingDefOf_SecuritronLoadout
    {
        [MayRequire("Rick.FCP.Robotics")]
        public static ThingDef FCP_Apparel_Securitron_RocketPod;
        [MayRequire("Rick.FCP.Robotics")]
        public static ThingDef FCP_Gun_Securitron_Arm;
        [MayRequire("Rick.FCP.Robotics")]
        public static ThingDef FCP_Gun_Securitron_GrenadeLauncher;

        static ThingDefOf_SecuritronLoadout()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(ThingDefOf_SecuritronLoadout));
        }
    }
}
