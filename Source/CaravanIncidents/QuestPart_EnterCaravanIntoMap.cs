using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.Noise;

namespace FCP_CaravanIncidents
{
    public class QuestPart_EnterCaravanIntoMap : QuestPart
    {
        public Caravan caravan;
        public int tile;
        public string inSignal;

        public override void Notify_QuestSignalReceived(Signal signal)
        {
            base.Notify_QuestSignalReceived(signal);
            Log.Message("QuestPart");
            Log.Message(caravan == null);
            Log.Message(tile == 0);

            Log.Message(tile);
            Log.Message(Find.WorldObjects.AnySiteAt(tile));
            if (caravan != null && tile != 0 && Find.WorldObjects.AnySiteAt(tile))
            {

                Site site =  Find.WorldObjects.SiteAt(tile);
                Log.Message(site.Label);
                caravan.Tile = site.Tile;
                caravan.pather.StopDead();
                CaravanArrivalAction_VisitSite action = new CaravanArrivalAction_VisitSite(site);
                action.Arrived(caravan);
            }
        }
        private bool TryFindWalkInSpot(Map map, out IntVec3 spawnSpot)
        {
            if (CellFinder.TryFindRandomEdgeCellWith((IntVec3 c) => !c.Fogged(map) && map.reachability.CanReachColony(c), map, CellFinder.EdgeRoadChance_Neutral, out spawnSpot))
            {
                return true;
            }
            if (CellFinder.TryFindRandomEdgeCellWith((IntVec3 c) => !c.Fogged(map), map, CellFinder.EdgeRoadChance_Neutral, out spawnSpot))
            {
                return true;
            }
            if (CellFinder.TryFindRandomEdgeCellWith((IntVec3 c) => true, map, CellFinder.EdgeRoadChance_Neutral, out spawnSpot))
            {
                return true;
            }
            return false;
        }


        //
        //CaravanEnterMapUtility.Enter(car, map, (Pawn x) => CellFinder.RandomSpawnCellForPawnNear(playerStartingSpot, map), CaravanDropInventoryMode.DoNotDrop, draftColonists: true);
    }
}
