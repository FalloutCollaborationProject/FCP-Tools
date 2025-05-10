using Verse.AI;

namespace FCP.Core
{
    public class JobGiver_ReactToDistantThreat : ThinkNode_JobGiver
    {
        private const float minEnemySearchDist = 3f;
        private const float maxEnemySearchDist = 9999f;
        private const float maxTravelRadius = 9999f;

        protected override Job TryGiveJob(Pawn pawn)
        {
            if (!IsValidPawnForReaction(pawn)) 
                return null;
            
            if (IsCurrentlyReacting(pawn))
                return null;
            
            Pawn attacker = FindAttacker(pawn);
                
            if (attacker == null || 
                attacker.IsPsychologicallyInvisible() || 
                PawnUtility.PlayerForcedJobNowOrSoon(pawn))
                return null;
            
            Ability ability = GetRandomAbility(pawn);
            return ability?.verb == null 
                ? null 
                : CreateAbilityJob(attacker, ability);
        }
        
        private static bool IsValidPawnForReaction(Pawn pawn)
        {
            return !pawn.Downed && !pawn.Dead && 
                   pawn.RaceProps.Animal && pawn.abilities != null;
        }
        
        private static bool IsCurrentlyReacting(Pawn pawn)
        {
            Job curJob = pawn.CurJob;
            return curJob != null && pawn.jobs.curDriver is JobDriver_AICastAbilityAtTarget;
        }
        
        private Pawn FindAttacker(Pawn pawn)
        {
            return (Pawn)AttackTargetFinder.BestAttackTarget(pawn,
                TargetScanFlags.NeedThreat | TargetScanFlags.NeedLOSToPawns | TargetScanFlags.NeedAutoTargetable,
                thing => thing is Pawn enemyPawn && enemyPawn.HostileTo(pawn),
                minEnemySearchDist, maxEnemySearchDist, pawn.Position, maxTravelRadius,
                false, false);
        }
        
        private static Ability GetRandomAbility(Pawn pawn)
        {
            return pawn.abilities?.abilities?.RandomElement();
        }
        
        private static Job CreateAbilityJob(Pawn attacker, Ability ability)
        {
            Job job = JobMaker.MakeJob(FCPDefOf.FCP_AICastAbilityAtTarget, attacker);
            job.locomotionUrgency = LocomotionUrgency.Jog;
            job.ability = ability;
            job.verbToUse = ability.verb;
            return job;
        }
    }
}