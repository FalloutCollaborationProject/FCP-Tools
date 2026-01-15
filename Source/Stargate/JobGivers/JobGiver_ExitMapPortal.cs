using Verse.AI;

namespace Thek_BuildingArrivalMode
{
    public class JobGiver_ExitMapPortal : ThinkNode_JobGiver
    {
        // Most of this is copied from JobGiver_ExitMap

        protected bool forceCanDig;
        protected bool forceCanDigIfCantReachMapEdge;
        protected bool forceCanDigIfAnyHostileActiveThreat;
        protected int jobMaxDuration = 999999;
        protected bool failIfCantJoinOrCreateCaravan;
        protected LocomotionUrgency defaultLocomotion;
        protected bool canBash;


        protected override Job TryGiveJob(Pawn pawn)
        {
            if (!pawn.CanReach(PawnsArrivalModeWorker_BuildingArrivalMode.modExtension.tileToSpawn, PathEndMode.OnCell, Danger.Unspecified))
            {
                using PawnPath path = pawn.Map.pathFinder.FindPath(pawn.Position, PawnsArrivalModeWorker_BuildingArrivalMode.modExtension.tileToSpawn, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.PassAllDestroyableThings));
                // Tries to do a path passing through destructible things if it's faster
                Thing thing = path.FirstBlockingBuilding(out IntVec3 cellBefore, pawn);
                // And if it finds something that blocks that path it just destroys it
                if (thing != null)
                {
                    Job digThroughBlockage = DigUtility.PassBlockerJob(pawn, thing, cellBefore, true, true);
                    if (digThroughBlockage != null)
                    {
                        return digThroughBlockage;
                    }
                }
            }
            // Uses the custom goto job to go to the cell where the raiders spawn from
            Job tryExit = JobMaker.MakeJob(JobDefOfs.Thek_GotoNoExitCellCheck, PawnsArrivalModeWorker_BuildingArrivalMode.modExtension.tileToSpawn);
            tryExit.exitMapOnArrival = true;
            tryExit.failIfCantJoinOrCreateCaravan = failIfCantJoinOrCreateCaravan;
            tryExit.locomotionUrgency = PawnUtility.ResolveLocomotion(pawn, defaultLocomotion, LocomotionUrgency.Jog);
            tryExit.expiryInterval = jobMaxDuration;
            tryExit.canBashDoors = canBash;
            return tryExit;
        }

        public override ThinkNode DeepCopy(bool resolve = true)
        {
            JobGiver_ExitMapPortal obj = (JobGiver_ExitMapPortal)base.DeepCopy(resolve);
            obj.defaultLocomotion = defaultLocomotion;
            obj.jobMaxDuration = jobMaxDuration;
            obj.canBash = canBash;
            obj.forceCanDig = forceCanDig;
            obj.forceCanDigIfAnyHostileActiveThreat = forceCanDigIfAnyHostileActiveThreat;
            obj.forceCanDigIfCantReachMapEdge = forceCanDigIfCantReachMapEdge;
            obj.failIfCantJoinOrCreateCaravan = failIfCantJoinOrCreateCaravan;
            return obj;
        }
    }
}