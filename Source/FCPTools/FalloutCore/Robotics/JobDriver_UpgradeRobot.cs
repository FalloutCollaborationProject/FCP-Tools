using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace FCP.Core.Robotics
{
    public class JobDriver_UpgradeRobot : JobDriver
    {
        private const int WorkTicks = 320;

        private Thing Bench => job.GetTarget(TargetIndex.A).Thing;
        private Pawn Robot => (Pawn)job.GetTarget(TargetIndex.B).Thing;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(Bench, job, 1, -1, null, errorOnFailed) && pawn.Reserve(Robot, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull(TargetIndex.A);
            this.FailOnDestroyedOrNull(TargetIndex.B);
            this.FailOn(() => Robot.CurJobDef != JobDefOf_Robotics.FCP_RobotDock);

            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);

            Toil work = Toils_General.Wait(WorkTicks);
            work.WithProgressBarToilDelay(TargetIndex.A);
            work.FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell);
            yield return work;

            yield return new Toil
            {
                initAction = FinishUpgrade,
                defaultCompleteMode = ToilCompleteMode.Instant,
            };
        }

        private void FinishUpgrade()
        {
            IRobotTierProvider provider = RobotUtility.GetProvider(Robot);
            PawnKindDef nextTier = provider?.GetNextTier(Robot.kindDef);
            if (nextTier == null)
            {
                return;
            }

            RobotTierExtension tierExt = nextTier.GetModExtension<RobotTierExtension>();
            if (tierExt != null && !RobotUpgradeUtility.TryConsumeCost(pawn.Map, tierExt.upgradeCost))
            {
                return;
            }

            provider.UpgradeTo(Robot, nextTier);

            CompRobotUpgrade upgradeComp = Robot.GetComp<CompRobotUpgrade>();
            if (upgradeComp != null)
            {
                upgradeComp.PendingBench = null;
            }
            if (Robot.CurJobDef == JobDefOf_Robotics.FCP_RobotDock)
            {
                Robot.jobs.EndCurrentJob(JobCondition.InterruptForced);
            }
        }
    }
}
