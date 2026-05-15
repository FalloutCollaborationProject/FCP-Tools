using Verse.AI;

namespace FCP.Core;

public class JobGiver_HostileAnimal : ThinkNode_JobGiver
{
    public float maxProximityDistance = 20f;
    public float maxRetaliationDistance = 40f;

    protected override Job TryGiveJob(Pawn pawn)
    {
        Pawn target = FindTarget(pawn);
        if (target == null || !pawn.CanReach(target, PathEndMode.Touch, Danger.Deadly))
            return null;

        return JobMaker.MakeJob(JobDefOf.AttackMelee, target);
    }

    private Pawn FindTarget(Pawn pawn)
    {
        Thing lastTarget = pawn.mindState.lastAttackedTarget.Thing;
        if (lastTarget is Pawn attacker && !attacker.Dead && !attacker.Downed
            && attacker.SpawnedOrAnyParentSpawned && pawn.HostileTo(attacker)
            && pawn.Position.DistanceTo(attacker.Position) <= maxRetaliationDistance)
            return attacker;

        return (Pawn)AttackTargetFinder.BestAttackTarget(
            pawn,
            TargetScanFlags.NeedThreat | TargetScanFlags.NeedLOSToPawns | TargetScanFlags.NeedAutoTargetable,
            t => t is Pawn p && pawn.HostileTo(p),
            0f, maxProximityDistance, pawn.Position, maxProximityDistance,
            false, false);
    }

    public override ThinkNode DeepCopy(bool resolve = true)
    {
        JobGiver_HostileAnimal copy = (JobGiver_HostileAnimal)base.DeepCopy(resolve);
        copy.maxProximityDistance = maxProximityDistance;
        copy.maxRetaliationDistance = maxRetaliationDistance;
        return copy;
    }
}
