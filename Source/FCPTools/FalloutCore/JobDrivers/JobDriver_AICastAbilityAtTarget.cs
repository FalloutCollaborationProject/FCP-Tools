using Verse.AI;

namespace FCP.Core
{
    public class JobDriver_AICastAbilityAtTarget : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            if (job.targetA.Thing is IAttackTarget target)
            {
                pawn.Map.attackTargetReservationManager.Reserve(pawn, job, target);
            }
            return true;
        }
        
        public override IEnumerable<Toil> MakeNewToils()
        {
            if (job.ability == null || job.verbToUse == null) 
                yield break;
            
            this.FailOnDespawnedOrNull(TargetIndex.A);
            this.FailOn(() => TargetA.Thing is Pawn { Dead: true });
            
            AddFinishAction(ApplyCooldownIfAbilityExists);
            
            yield return CreateMoveToTargetToil();
            yield return CreateStopMovingToil();
            yield return CreateCastAbilityToil();
        }
        
        private Toil CreateMoveToTargetToil()
        {
            return new Toil
            {
                initAction = () =>
                {
                    float maxRange = job.ability.verb.verbProps.range;
                    const float minRange = 4f;
                    
                    IntVec3 targetPos = TargetA.Cell;
                    IntVec3 destination = GetValidPositionNearTarget(targetPos, maxRange, minRange);
                    
                    if (destination.IsValid)
                    {
                        pawn.pather.StartPath(destination, PathEndMode.OnCell);
                    }
                    else
                    {
                        FCPLog.Warning("No valid position found within the range.");
                        EndJobWith(JobCondition.Incompletable);
                    }
                },
                defaultCompleteMode = ToilCompleteMode.PatherArrival
            };
        }
        
        private Toil CreateStopMovingToil()
        {
            return new Toil
            {
                initAction = () => pawn.pather.StopDead(),
                defaultCompleteMode = ToilCompleteMode.Instant
            };
        }
        
        private Toil CreateCastAbilityToil()
        {
            Toil castAbility = Toils_Combat.CastVerb(TargetIndex.A,
                TargetIndex.B, canHitNonTargetPawns: false);
            
            if (job.ability != null && 
                job.ability.def.showCastingProgressBar && 
                job.verbToUse != null)
            {
                castAbility.WithProgressBar(TargetIndex.A, () => 
                    job.verbToUse.WarmupProgress);
            }
            return castAbility;
        }
        
        private IntVec3 GetValidPositionNearTarget(IntVec3 targetPos, float maxRange, float minRange)
        {
            for (int i = 0; i < 30; i++)
            {
                IntVec3 randomPos = targetPos + GenRadial.RadialPattern[
                    Rand.Range(0, GenRadial.NumCellsInRadius(maxRange))
                ];
                float distanceToTarget = (randomPos - targetPos).LengthHorizontal;
                
                if (distanceToTarget <= maxRange && 
                    distanceToTarget >= minRange && 
                    randomPos.InBounds(pawn.Map) && 
                    randomPos.Standable(pawn.Map))
                {
                    return randomPos;
                }
            }
            return IntVec3.Invalid;
        }
        
        private void ApplyCooldownIfAbilityExists(JobCondition jobCondition)
        {
            if (job.ability != null && job.def.abilityCasting)
            {
                job.ability.StartCooldown(job.ability.def.cooldownTicksRange.RandomInRange);
            }
        }
        
        public override bool IsContinuation(Job j)
        {
            return job.GetTarget(TargetIndex.A) == j.GetTarget(TargetIndex.A);
        }
    }
}