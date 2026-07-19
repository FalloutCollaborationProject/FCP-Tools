using RimWorld;
using Verse;
using Verse.AI;

namespace FCP.Core.Robotics
{
    public class JobGiver_MrHandyMode : JobGiver_AIFightEnemy
    {
        private const float FollowRadius = 6f;
        private const float GuardEngageRadius = 15f;

        protected override Job TryGiveJob(Pawn pawn)
        {
            CompRobotUpgrade upgradeComp = pawn.GetComp<CompRobotUpgrade>();
            if (upgradeComp?.PendingBench != null)
            {
                if (!upgradeComp.PendingBench.Spawned)
                {
                    upgradeComp.PendingBench = null;
                }
                else
                {
                    return JobMaker.MakeJob(JobDefOf_Robotics.FCP_RobotDock, upgradeComp.PendingBench);
                }
            }

            CompRefuelable fuel = pawn.GetComp<CompRefuelable>();
            if (fuel != null && !fuel.HasFuel)
            {
                return null;
            }

            if (!RobotUtility.IsPoweredOn(pawn))
            {
                return null;
            }

            if (pawn.Faction == null)
            {
                return null;
            }

            CompMrHandyMode modeComp = pawn.GetComp<CompMrHandyMode>();
            CompMrHandyLoadout loadout = pawn.GetComp<CompMrHandyLoadout>();
            if (modeComp == null || loadout == null || !loadout.SupportsMode(modeComp.Mode))
            {
                return null;
            }

            switch (modeComp.Mode)
            {
                case MrHandyMode.Guard:
                    return base.TryGiveJob(pawn);
                case MrHandyMode.GuardPawn:
                    return TryGuardPawn(pawn, modeComp);
                case MrHandyMode.Cook:
                    return TryWorkGiver(pawn, "DoBillsCook", ThingRequestGroup.BuildingArtificial)
                        ?? TryWorkGiver(pawn, "DoBillsCookCampfire", ThingRequestGroup.BuildingArtificial);
                case MrHandyMode.Clean:
                    return TryWorkGiver(pawn, "CleanFilth", ThingRequestGroup.Filth);
                case MrHandyMode.Garden:
                    return TryCellWorkGiver(pawn, "GrowerSow") ?? TryCellWorkGiver(pawn, "GrowerHarvest");
                case MrHandyMode.BasicCare:
                    return TryWorkGiver(pawn, "DoctorFeedHumanlikes", ThingRequestGroup.Pawn);
                case MrHandyMode.Tend:
                    return TryWorkGiver(pawn, "DoctorTendToHumanlikes", ThingRequestGroup.Pawn);
                case MrHandyMode.Childcare:
                    return TryWorkGiver(pawn, "BottleFeedBaby", ThingRequestGroup.Pawn)
                        ?? TryWorkGiver(pawn, "PlayWithBaby", ThingRequestGroup.Pawn);
                default:
                    return null;
            }
        }

        private Job TryGuardPawn(Pawn pawn, CompMrHandyMode modeComp)
        {
            if (modeComp.GuardedPawn == null || !modeComp.GuardedPawn.Spawned)
            {
                return null;
            }

            Job fightJob = base.TryGiveJob(pawn);
            if (fightJob != null)
            {
                return fightJob;
            }
            if (!pawn.Position.InHorDistOf(modeComp.GuardedPawn.Position, FollowRadius))
            {
                return JobMaker.MakeJob(JobDefOf.Goto, modeComp.GuardedPawn.Position);
            }
            return null;
        }

        protected override bool ExtraTargetValidator(Pawn pawn, Thing target)
        {
            if (!base.ExtraTargetValidator(pawn, target))
            {
                return false;
            }

            CompMrHandyMode modeComp = pawn.GetComp<CompMrHandyMode>();
            if (modeComp?.Mode == MrHandyMode.GuardPawn && modeComp.GuardedPawn != null)
            {
                return target.Position.InHorDistOf(modeComp.GuardedPawn.Position, GuardEngageRadius);
            }

            return pawn.Map.areaManager.Home[target.Position];
        }

        protected override bool TryFindShootingPosition(Pawn pawn, out IntVec3 dest, Verb verbToUse = null)
        {
            Thing enemyTarget = pawn.mindState.enemyTarget;
            Verb verb = verbToUse ?? pawn.TryGetAttackVerb(enemyTarget, !pawn.IsColonist);
            if (verb == null)
            {
                dest = IntVec3.Invalid;
                return false;
            }

            return CastPositionFinder.TryFindCastPosition(new CastPositionRequest
            {
                caster = pawn,
                target = enemyTarget,
                verb = verb,
                maxRangeFromTarget = 9999f,
                locus = pawn.Position,
                maxRangeFromLocus = 9999f,
                wantCoverFromTarget = verb.EffectiveRange > 7f,
            }, out dest);
        }

        private static Job TryWorkGiver(Pawn pawn, string workGiverDefName, ThingRequestGroup group)
        {
            WorkGiverDef def = DefDatabase<WorkGiverDef>.GetNamedSilentFail(workGiverDefName);
            if (!(def?.Worker is WorkGiver_Scanner giver))
            {
                return null;
            }

            Thing target = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map,
                ThingRequest.ForGroup(group), PathEndMode.Touch,
                TraverseParms.For(pawn), 9999f,
                thing => giver.HasJobOnThing(pawn, thing));

            return target == null ? null : giver.JobOnThing(pawn, target);
        }

        private static Job TryCellWorkGiver(Pawn pawn, string workGiverDefName)
        {
            WorkGiverDef def = DefDatabase<WorkGiverDef>.GetNamedSilentFail(workGiverDefName);
            if (!(def?.Worker is WorkGiver_Scanner giver))
            {
                return null;
            }

            foreach (IntVec3 cell in giver.PotentialWorkCellsGlobal(pawn))
            {
                if (giver.HasJobOnCell(pawn, cell))
                {
                    return giver.JobOnCell(pawn, cell);
                }
            }
            return null;
        }
    }
}
