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
            // This makes it leave the map iirc it won't let you do so unless this is true
            Toil toil = Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
            // Goto to the cell where they spawned from initially
            toil.AddFinishAction(delegate
            {
                if (job.controlGroupTag != null)
                // I copied this from the regular goto.
                {
                    pawn.GetOverseer()?.mechanitor.GetControlGroup(pawn).SetTag(pawn, job.controlGroupTag);
                }
            });
            yield return toil;
            Toil toil2 = ToilMaker.MakeToil("MakeNewToils");
            //This one runs once the previous toil finishes, eg when the pawn has reached the cell or otherwise interrumpted
            toil2.initAction = delegate
            {
                if (pawn.mindState != null && pawn.mindState.forcedGotoPosition == TargetA.Cell)
                {
                    pawn.mindState.forcedGotoPosition = IntVec3.Invalid;
                }
                if (job.exitMapOnArrival && pawn.Position == PawnsArrivalModeWorker_BuildingArrivalMode.modExtension.tileToSpawn)
                {
                    // PawnsArrivalModeWorker_BuildingArrivalMode.modExtension.tileToSpawn is the same cell where they have spawned from
                    TryExitMap();
                }
            };
            toil2.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return toil2;
        }

        /// <summary>
        /// Most of this method is copied from the vanilla goto, without the checks requiring the pawn being on the tile border
        /// </summary>
        internal void TryExitMap()
        {
            // This is for colonist pawns only i think, it might be safe to delete tbf
            if (!job.failIfCantJoinOrCreateCaravan || CaravanExitMapUtility.CanExitMapAndJoinOrCreateCaravanNow(pawn))
            {
                if (ModsConfig.BiotechActive)
                {
                    MechanitorUtility.Notify_PawnGotoLeftMap(pawn, pawn.Map);
                }

                PawnsArrivalModeWorker_BuildingArrivalMode.modExtension.soundWhenSpawning?.PlayOneShot(new TargetInfo(pawn.Position, pawn.Map));
                //If you've not specified a sound, it won't do anything. But if you have, it'll just play it.
                if (PawnsArrivalModeWorker_BuildingArrivalMode.modExtension?.fleckWhenSpawning != null) pawn.Map.flecks.CreateFleck(FleckMaker.GetDataStatic(pawn.DrawPos, pawn.Map, PawnsArrivalModeWorker_BuildingArrivalMode.modExtension?.fleckWhenSpawning));
                //Same with the fleck
                pawn.ExitMap(allowedToJoinOrCreateCaravan: true, CellRect.WholeMap(base.Map).GetClosestEdge(pawn.Position));
            }
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }
    }
}
