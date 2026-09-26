using RimWorld;
using Verse;
using Verse.AI;

namespace FCP.Core.Robotics
{
    public class WorkGiver_UpgradeRobot : WorkGiver_Scanner
    {
        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial);

        public override PathEndMode PathEndMode => PathEndMode.InteractionCell;

        public override Danger MaxPathDanger(Pawn pawn)
        {
            return Danger.Deadly;
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (!(t is Building building) || building.GetComp<CompRobotUpgradeBench>() == null)
            {
                return false;
            }

            Pawn robot = CompRobotUpgradeBench.FindDockedRobotAt(building);
            if (robot == null)
            {
                return false;
            }

            IRobotTierProvider provider = RobotUtility.GetProvider(robot);
            PawnKindDef nextTier = provider?.GetNextTier(robot.kindDef);
            if (nextTier == null)
            {
                return false;
            }

            RobotTierExtension tierExt = nextTier.GetModExtension<RobotTierExtension>();
            if (tierExt != null && !RobotUpgradeUtility.CanAffordCost(building.Map, tierExt.upgradeCost))
            {
                JobFailReason.Is("FCP_UpgradeRobot_MissingMaterials".Translate());
                return false;
            }

            return pawn.CanReserve(building, 1, -1, null, forced) && pawn.CanReserve(robot, 1, -1, null, forced);
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            Building building = (Building)t;
            Pawn robot = CompRobotUpgradeBench.FindDockedRobotAt(building);
            return robot == null ? null : JobMaker.MakeJob(JobDefOf_Robotics.FCP_UpgradeRobot, building, robot);
        }
    }
}
