using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace FCP.Core.Robotics
{
    public class CompProperties_RobotUpgrade : CompProperties
    {
        public CompProperties_RobotUpgrade()
        {
            compClass = typeof(CompRobotUpgrade);
        }
    }

    public class CompRobotUpgrade : ThingComp
    {
        public Thing PendingBench;

        public CompProperties_RobotUpgrade Props => (CompProperties_RobotUpgrade)props;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_References.Look(ref PendingBench, "pendingUpgradeBench");
        }

        public override string CompInspectStringExtra()
        {
            if (PendingBench == null)
            {
                return null;
            }
            return "FCP_UpgradeRobot_CalledToBench".Translate();
        }

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            Pawn pawn = parent as Pawn;
            if (pawn == null || PendingBench == null || pawn.Faction != Faction.OfPlayer)
            {
                yield break;
            }

            if (!selPawn.CanReach(parent, PathEndMode.Touch, Danger.Deadly))
            {
                yield break;
            }

            yield return new FloatMenuOption("FCP_UpgradeRobot_Release".Translate(), delegate
            {
                PendingBench = null;
            });
        }
    }
}
