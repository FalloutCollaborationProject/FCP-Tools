using RimWorld;
using Verse;

namespace FCP.Core.Robotics
{
    [DefOf]
    public static class PawnKindDefOf_Eyebot
    {
        [MayRequire("Rick.FCP.Robotics")]
        public static PawnKindDef FCP_Pawnkind_Eyebot_Normal;
        [MayRequire("Rick.FCP.Robotics")]
        public static PawnKindDef FCP_Pawnkind_Eyebot_Improved;
        [MayRequire("Rick.FCP.Robotics")]
        public static PawnKindDef FCP_Pawnkind_Eyebot_LonesomeRoad;

        static PawnKindDefOf_Eyebot()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(PawnKindDefOf_Eyebot));
        }
    }

    [DefOf]
    public static class HediffDefOf_Eyebot
    {
        [MayRequire("Rick.FCP.Robotics")]
        public static HediffDef FCP_Hediff_Eyebot_Tier_Normal;
        [MayRequire("Rick.FCP.Robotics")]
        public static HediffDef FCP_Hediff_Eyebot_Tier_Improved;
        [MayRequire("Rick.FCP.Robotics")]
        public static HediffDef FCP_Hediff_Eyebot_Tier_LonesomeRoad;

        static HediffDefOf_Eyebot()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(HediffDefOf_Eyebot));
        }
    }

    [DefOf]
    public static class AbilityDefOf_Eyebot
    {
        [MayRequire("Rick.FCP.Robotics")]
        public static AbilityDef FCP_Ability_Eyebot_Laser_Normal;
        [MayRequire("Rick.FCP.Robotics")]
        public static AbilityDef FCP_Ability_Eyebot_Laser_Improved;
        [MayRequire("Rick.FCP.Robotics")]
        public static AbilityDef FCP_Ability_Eyebot_Laser_LonesomeRoad;

        static AbilityDefOf_Eyebot()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(AbilityDefOf_Eyebot));
        }
    }

    public static class EyebotTierUtility
    {
        public static PawnKindDef[] TierOrder => new[]
        {
            PawnKindDefOf_Eyebot.FCP_Pawnkind_Eyebot_Normal,
            PawnKindDefOf_Eyebot.FCP_Pawnkind_Eyebot_Improved,
            PawnKindDefOf_Eyebot.FCP_Pawnkind_Eyebot_LonesomeRoad,
        };

        public static HediffDef[] HediffOrder => new[]
        {
            HediffDefOf_Eyebot.FCP_Hediff_Eyebot_Tier_Normal,
            HediffDefOf_Eyebot.FCP_Hediff_Eyebot_Tier_Improved,
            HediffDefOf_Eyebot.FCP_Hediff_Eyebot_Tier_LonesomeRoad,
        };

        public static AbilityDef[] AbilityOrder => new[]
        {
            AbilityDefOf_Eyebot.FCP_Ability_Eyebot_Laser_Normal,
            AbilityDefOf_Eyebot.FCP_Ability_Eyebot_Laser_Improved,
            AbilityDefOf_Eyebot.FCP_Ability_Eyebot_Laser_LonesomeRoad,
        };

        public static bool IsEyebot(Pawn pawn)
        {
            return pawn?.kindDef != null && System.Array.IndexOf(TierOrder, pawn.kindDef) >= 0;
        }

        public static int TierIndex(PawnKindDef kind)
        {
            return System.Array.IndexOf(TierOrder, kind);
        }

        public static PawnKindDef GetNextTier(PawnKindDef current)
        {
            int index = TierIndex(current);
            if (index < 0 || index >= TierOrder.Length - 1)
            {
                return null;
            }
            return TierOrder[index + 1];
        }

        public static void UpgradeTo(Pawn eyebot, PawnKindDef nextTier)
        {
            int currentIndex = TierIndex(eyebot.kindDef);
            if (currentIndex >= 0)
            {
                Hediff oldTierHediff = eyebot.health.hediffSet.hediffs.Find(h => h.def == HediffOrder[currentIndex]);
                if (oldTierHediff != null)
                {
                    eyebot.health.RemoveHediff(oldTierHediff);
                }
            }

            if (currentIndex >= 0 && eyebot.abilities != null)
            {
                eyebot.abilities.RemoveAbility(AbilityOrder[currentIndex]);
            }

            eyebot.kindDef = nextTier;

            int nextIndex = TierIndex(nextTier);
            if (nextIndex >= 0)
            {
                eyebot.health.AddHediff(HediffOrder[nextIndex]);
                eyebot.abilities?.GainAbility(AbilityOrder[nextIndex]);
            }

            eyebot.Drawer.renderer.SetAllGraphicsDirty();
        }
    }
}
