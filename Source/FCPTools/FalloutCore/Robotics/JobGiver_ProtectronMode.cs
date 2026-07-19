using RimWorld;
using Verse;
using Verse.AI;

namespace FCP.Core.Robotics
{
    public class JobGiver_ProtectronMode : JobGiver_AIFightEnemy
    {
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

            CompProtectronMode modeComp = pawn.GetComp<CompProtectronMode>();
            if (modeComp == null)
            {
                return null;
            }

            switch (modeComp.Mode)
            {
                case ProtectronMode.Guard:
                    return CompProtectronLoadout.HasHand(pawn, HediffDefOf_Protectron.FCP_Hediff_Protectron_Hand_Gun)
                        ? base.TryGiveJob(pawn)
                        : null;
                case ProtectronMode.Construct:
                    bool hasConstructHead = CompProtectronLoadout.HasHead(pawn, HediffDefOf_Protectron.FCP_Hediff_Protectron_Head_Construct);
                    bool hasWorkHand = CompProtectronLoadout.HasHand(pawn, HediffDefOf_Protectron.FCP_Hediff_Protectron_Hand_Work);
                    return hasConstructHead && hasWorkHand ? TryConstruct(pawn) : null;
                case ProtectronMode.Haul:
                    return CompProtectronLoadout.HasHand(pawn, HediffDefOf_Protectron.FCP_Hediff_Protectron_Hand_Default)
                        ? TryHaul(pawn)
                        : null;
                default:
                    return null;
            }
        }

        protected override bool ExtraTargetValidator(Pawn pawn, Thing target)
        {
            if (!base.ExtraTargetValidator(pawn, target))
            {
                return false;
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

        private static Job TryHaul(Pawn pawn)
        {
            WorkGiver_HaulGeneral haulGiver = (WorkGiver_HaulGeneral)DefDatabase<WorkGiverDef>.GetNamed("HaulGeneral").Worker;
            Thing haulable = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map,
                ThingRequest.ForGroup(ThingRequestGroup.HaulableEver), PathEndMode.ClosestTouch,
                TraverseParms.For(pawn), 9999f,
                thing => haulGiver.HasJobOnThing(pawn, thing));

            return haulable == null ? null : haulGiver.JobOnThing(pawn, haulable);
        }

        private static Job TryConstruct(Pawn pawn)
        {
            return TryWorkGiver(pawn, "ConstructDeliverResourcesToBlueprints", ThingRequestGroup.Blueprint)
                ?? TryWorkGiver(pawn, "ConstructDeliverResourcesToFrames", ThingRequestGroup.BuildingFrame)
                ?? TryWorkGiver(pawn, "ConstructFinishFrames", ThingRequestGroup.BuildingFrame);
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
    }
}
