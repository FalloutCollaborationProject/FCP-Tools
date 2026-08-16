using RimWorld.Planet;
using Verse.AI;
using Verse.Sound;

namespace Thek_BuildingArrivalMode
{
    internal class JobDriver_GotoNoExitCellCheck : JobDriver
    {
        protected override IEnumerable<Toil> MakeNewToils()
        {
            job.exitMapOnArrival = true;
            Toil toil = Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
            toil.AddFinishAction(delegate
            {
                if (job.controlGroupTag != null)
                {
                    pawn.GetOverseer()?.mechanitor.GetControlGroup(pawn).SetTag(pawn, job.controlGroupTag);
                }
            });
            yield return toil;
            Toil toil2 = ToilMaker.MakeToil("MakeNewToils");
            toil2.initAction = delegate
            {
                if (pawn.mindState != null && pawn.mindState.forcedGotoPosition == TargetA.Cell)
                {
                    pawn.mindState.forcedGotoPosition = IntVec3.Invalid;
                }
                if (job.exitMapOnArrival && pawn.Position == PawnsArrivalModeWorker_BuildingArrivalMode.modExtension.tileToSpawn)
                {
                    TryExitMap();
                }
            };
            toil2.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return toil2;
        }

        internal void TryExitMap()
        {
            if (!job.failIfCantJoinOrCreateCaravan || CaravanExitMapUtility.CanExitMapAndJoinOrCreateCaravanNow(pawn))
            {
                if (ModsConfig.BiotechActive)
                {
                    MechanitorUtility.Notify_PawnGotoLeftMap(pawn, pawn.Map);
                }

                PawnsArrivalModeWorker_BuildingArrivalMode.modExtension.soundWhenSpawning?.PlayOneShot(new TargetInfo(pawn.Position, pawn.Map));
                if (PawnsArrivalModeWorker_BuildingArrivalMode.modExtension?.fleckWhenSpawning != null) pawn.Map.flecks.CreateFleck(FleckMaker.GetDataStatic(pawn.DrawPos, pawn.Map, PawnsArrivalModeWorker_BuildingArrivalMode.modExtension?.fleckWhenSpawning));
                pawn.ExitMap(allowedToJoinOrCreateCaravan: true, CellRect.WholeMap(base.Map).GetClosestEdge(pawn.Position));
            }
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }
    }
}
