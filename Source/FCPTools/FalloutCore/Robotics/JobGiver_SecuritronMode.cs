using RimWorld;
using Verse;
using Verse.AI;

namespace FCP.Core.Robotics
{
    public class JobGiver_SecuritronMode : JobGiver_AIFightEnemy
    {
        private const float FollowRadius = 6f;
        private const float GuardEngageRadius = 15f;

        protected override Job TryGiveJob(Pawn pawn)
        {
            CompRefuelable fuel = pawn.GetComp<CompRefuelable>();
            if (fuel != null && !fuel.HasFuel)
            {
                return null;
            }

            if (pawn.Faction == null)
            {
                return null;
            }

            CompSecuritronMode modeComp = pawn.GetComp<CompSecuritronMode>();
            if (modeComp == null)
            {
                return null;
            }

            if (modeComp.Mode == SecuritronMode.GuardPawn && modeComp.GuardedPawn != null && modeComp.GuardedPawn.Spawned)
            {
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

            return base.TryGiveJob(pawn);
        }

        protected override bool ExtraTargetValidator(Pawn pawn, Thing target)
        {
            if (!base.ExtraTargetValidator(pawn, target))
            {
                return false;
            }

            CompSecuritronMode modeComp = pawn.GetComp<CompSecuritronMode>();
            if (modeComp?.Mode == SecuritronMode.GuardPawn && modeComp.GuardedPawn != null)
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
    }
}
