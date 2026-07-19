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

    public class MrHandyTierProvider : IRobotTierProvider
    {
        public bool IsThisRace(Pawn pawn) => pawn?.kindDef?.race == ThingDefOf_MrHandy.FCP_MrHandy;
        public PawnKindDef GetNextTier(PawnKindDef current) => null;
        public void UpgradeTo(Pawn robot, PawnKindDef nextTier) { }
    }

    public class SentryBotTierProvider : IRobotTierProvider
    {
        public bool IsThisRace(Pawn pawn) => pawn?.kindDef?.race == ThingDefOf_SentryBot.FCP_SentryBot;
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
            new MrHandyTierProvider(),
            new SentryBotTierProvider(),
        };

        public static bool IsAnyRobot(Pawn pawn) => Providers.Any(p => p.IsThisRace(pawn));

        public static IRobotTierProvider GetProvider(Pawn pawn) => Providers.FirstOrDefault(p => p.IsThisRace(pawn));

        public static bool IsPoweredOn(Pawn pawn)
        {
            CompRobotManualPower power = pawn.GetComp<CompRobotManualPower>();
            return power == null || power.PoweredOn;
        }

        public static bool HasRangedAttack(Pawn pawn)
        {
            if (pawn?.verbTracker?.AllVerbs != null &&
                pawn.verbTracker.AllVerbs.Any(v => v != null && !v.IsMeleeAttack && v.Available()))
            {
                return true;
            }

            if (pawn?.abilities?.AllAbilitiesForReading != null &&
                pawn.abilities.AllAbilitiesForReading.Any(a => a?.verb != null && !a.verb.IsMeleeAttack))
            {
                return true;
            }

            return false;
        }

        public static Building_Bed FindAssignedBed(Pawn robot)
        {
            Map map = robot?.MapHeld;
            if (map == null)
            {
                return null;
            }

            foreach (Building building in map.listerBuildings.allBuildingsColonist)
            {
                if (building is Building_Bed bed && (bed.CompAssignableToPawn?.AssignedPawnsForReading.Contains(robot) ?? false))
                {
                    return bed;
                }
            }

            return null;
        }

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
