using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace FCP.Core.Robotics
{
    public interface IRobotTierProvider
    {
        bool IsThisRace(Pawn pawn);
        PawnKindDef GetNextTier(PawnKindDef current);
        void UpgradeTo(Pawn robot, PawnKindDef nextTier);
    }

    public class EyebotTierProvider : IRobotTierProvider
    {
        public bool IsThisRace(Pawn pawn) => EyebotTierUtility.IsEyebot(pawn);
        public PawnKindDef GetNextTier(PawnKindDef current) => EyebotTierUtility.GetNextTier(current);
        public void UpgradeTo(Pawn robot, PawnKindDef nextTier) => EyebotTierUtility.UpgradeTo(robot, nextTier);
    }

    public class SecuritronTierProvider : IRobotTierProvider
    {
        public bool IsThisRace(Pawn pawn) => pawn?.kindDef?.race == ThingDefOf_Securitron.FCP_Securitron;
        public PawnKindDef GetNextTier(PawnKindDef current) => null;
        public void UpgradeTo(Pawn robot, PawnKindDef nextTier) { }
    }

    public class ProtectronTierProvider : IRobotTierProvider
    {
        public bool IsThisRace(Pawn pawn) => pawn?.kindDef?.race == ThingDefOf_Protectron.FCP_Protectron;
        public PawnKindDef GetNextTier(PawnKindDef current) => null;
        public void UpgradeTo(Pawn robot, PawnKindDef nextTier) { }
    }

    public static class RobotUtility
    {
        private static readonly List<IRobotTierProvider> Providers = new List<IRobotTierProvider>
        {
            new EyebotTierProvider(),
            new SecuritronTierProvider(),
            new ProtectronTierProvider(),
        };

        public static bool IsAnyRobot(Pawn pawn) => Providers.Any(p => p.IsThisRace(pawn));

        public static IRobotTierProvider GetProvider(Pawn pawn) => Providers.FirstOrDefault(p => p.IsThisRace(pawn));

        public static void TouchGraphic(Thing thing)
        {
            if (thing != null)
            {
                _ = thing.Graphic;
            }
        }

        public static void TouchBodyColor(Pawn pawn)
        {
            CompColorable colorable = pawn?.GetComp<CompColorable>();
            if (colorable == null || !colorable.Active)
            {
                return;
            }

            GraphicData data = pawn.kindDef?.lifeStages?.FirstOrDefault()?.bodyGraphicData;
            Graphic baseGraphic = data?.Graphic;
            if (baseGraphic == null)
            {
                return;
            }

            _ = baseGraphic.GetColoredVersion(baseGraphic.Shader, colorable.Color, baseGraphic.ColorTwo);
        }

        public static Color GetBodyColor(Pawn pawn)
        {
            CompColorable colorable = pawn?.GetComp<CompColorable>();
            if (colorable != null && colorable.Active)
            {
                return colorable.Color;
            }

            PawnKindLifeStage lifeStage = pawn?.kindDef?.lifeStages?.FirstOrDefault();
            if (lifeStage?.bodyGraphicData != null)
            {
                return lifeStage.bodyGraphicData.color;
            }

            return Color.white;
        }
    }
}
