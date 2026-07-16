using RimWorld;
using Verse;

namespace FCP.Core.Robotics
{
    [DefOf]
    public static class PawnKindDefOf_Protectron
    {
        [MayRequire("Rick.FCP.Robotics")]
        public static PawnKindDef FCP_Pawnkind_Protectron_VersionA;
        [MayRequire("Rick.FCP.Robotics")]
        public static PawnKindDef FCP_Pawnkind_Protectron_VersionB;
        [MayRequire("Rick.FCP.Robotics")]
        public static PawnKindDef FCP_Pawnkind_Protectron_Construction;
        [MayRequire("Rick.FCP.Robotics")]
        public static PawnKindDef FCP_Pawnkind_Protectron_Police;

        static PawnKindDefOf_Protectron()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(PawnKindDefOf_Protectron));
        }
    }

    [DefOf]
    public static class ThingDefOf_Protectron
    {
        [MayRequire("Rick.FCP.Robotics")]
        public static ThingDef FCP_Protectron;

        static ThingDefOf_Protectron()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(ThingDefOf_Protectron));
        }
    }

    [DefOf]
    public static class ThingDefOf_ProtectronLoadout
    {
        [MayRequire("Rick.FCP.Robotics")]
        public static ThingDef FCP_Apparel_Protectron_Head_Default;
        [MayRequire("Rick.FCP.Robotics")]
        public static ThingDef FCP_Apparel_Protectron_Head_Construct;
        [MayRequire("Rick.FCP.Robotics")]
        public static ThingDef FCP_Apparel_Protectron_Hand_Default;
        [MayRequire("Rick.FCP.Robotics")]
        public static ThingDef FCP_Apparel_Protectron_Hand_Work;
        [MayRequire("Rick.FCP.Robotics")]
        public static ThingDef FCP_Apparel_Protectron_Hand_Gun;
        [MayRequire("Rick.FCP.Robotics")]
        public static ThingDef FCP_Gun_Protectron_Arm;

        static ThingDefOf_ProtectronLoadout()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(ThingDefOf_ProtectronLoadout));
        }
    }
}
