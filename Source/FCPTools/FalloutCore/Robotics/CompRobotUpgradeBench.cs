using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace FCP.Core.Robotics
{
    public class CompProperties_RobotUpgradeBench : CompProperties
    {
        public CompProperties_RobotUpgradeBench()
        {
            compClass = typeof(CompRobotUpgradeBench);
        }
    }

    public class CompRobotUpgradeBench : ThingComp
    {
        public CompProperties_RobotUpgradeBench Props => (CompProperties_RobotUpgradeBench)props;

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            yield return new Command_Action
            {
                defaultLabel = "FCP_UpgradeRobot_Gizmo".Translate(),
                defaultDesc = "FCP_UpgradeRobot_GizmoDesc".Translate(),
                icon = ContentFinder<Texture2D>.Get("Things/Items/Techprints/FCP_Techprint_Holotape_Orange"),
                action = OpenUpgradeSelectMenu,
            };
        }

        private void OpenUpgradeSelectMenu()
        {
            List<FloatMenuOption> options = new List<FloatMenuOption>();
            IEnumerable<Pawn> robots = parent.Map.mapPawns.AllPawnsSpawned
                .Where(p => p.Faction == Faction.OfPlayer && RobotUtility.IsAnyRobot(p));

            foreach (Pawn robot in robots)
            {
                IRobotTierProvider provider = RobotUtility.GetProvider(robot);
                PawnKindDef nextTier = provider?.GetNextTier(robot.kindDef);
                if (nextTier != null)
                {
                    RobotTierExtension tierExt = nextTier.GetModExtension<RobotTierExtension>();
                    if (tierExt?.researchPrerequisite != null && !tierExt.researchPrerequisite.IsFinished)
                    {
                        options.Add(new FloatMenuOption($"{robot.LabelShort}: {"FCP_UpgradeRobot_NeedsResearch".Translate(tierExt.researchPrerequisite.LabelCap)}", null));
                        continue;
                    }
                }

                options.Add(new FloatMenuOption(robot.LabelShort, () => CallToBench(robot)));
            }

            if (options.Count == 0)
            {
                options.Add(new FloatMenuOption("FCP_UpgradeRobot_NoneAvailable".Translate(), null));
            }

            Find.WindowStack.Add(new FloatMenu(options));
        }

        private void CallToBench(Pawn robot)
        {
            CompRobotUpgrade upgradeComp = robot.GetComp<CompRobotUpgrade>();
            if (upgradeComp != null)
            {
                upgradeComp.PendingBench = parent;
            }
        }

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            if (!selPawn.CanReach(parent, PathEndMode.InteractionCell, Danger.Deadly))
            {
                yield return new FloatMenuOption("FCP_UpgradeRobot_Perform".Translate() + ": " + "NoPath".Translate(), null);
                yield break;
            }

            Pawn dockedRobot = FindDockedRobot();
            if (dockedRobot == null)
            {
                yield break;
            }

            IRobotTierProvider provider = RobotUtility.GetProvider(dockedRobot);
            if (provider == null)
            {
                yield break;
            }

            PawnKindDef nextTier = provider.GetNextTier(dockedRobot.kindDef);
            if (nextTier == null)
            {
                yield break;
            }

            RobotTierExtension tierExt = nextTier.GetModExtension<RobotTierExtension>();
            if (tierExt != null && !RobotUpgradeUtility.CanAffordCost(parent.Map, tierExt.upgradeCost))
            {
                yield return new FloatMenuOption("FCP_UpgradeRobot_Perform".Translate() + ": " + "FCP_UpgradeRobot_MissingMaterials".Translate(), null);
                yield break;
            }

            yield return new FloatMenuOption("FCP_UpgradeRobot_Perform".Translate(), delegate
            {
                Job job = JobMaker.MakeJob(JobDefOf_Robotics.FCP_UpgradeRobot, parent, dockedRobot);
                selPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
            });
        }

        private Pawn FindDockedRobot()
        {
            foreach (Pawn p in parent.Map.mapPawns.AllPawnsSpawned)
            {
                if (RobotUtility.IsAnyRobot(p) && p.CurJobDef == JobDefOf_Robotics.FCP_RobotDock &&
                    p.CurJob.GetTarget(TargetIndex.A).Thing == parent)
                {
                    return p;
                }
            }
            return null;
        }
    }
}
