using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace FCP_RadiantQuests
{
    public class QuestNode_SpawnBuilding : QuestNode
    {
        public SlateRef<Building> building;

        protected override bool TestRunInt(Slate slate)
        {
            return true;
        }

        protected override void RunInt()
        {
            Slate slate = QuestGen.slate;
            Log.Message("Getting map");
            Map map = QuestGen.slate.Get<Site>("site").Map;
            Log.Message(building.GetValue(slate).Label);
            Log.Message(map == null);
            foreach(IntVec3 cell in map.cellsInRandomOrder.GetAll())
            {
                if(cell.GetRoomOrAdjacent(map, RegionType.Normal) != null)
                {
                    GenPlace.TryPlaceThing(building.GetValue(slate), cell, map, ThingPlaceMode.Direct);
                    return;
                }
            }
            GenPlace.TryPlaceThing(building.GetValue(slate), map.Center, map, ThingPlaceMode.Near);
           
        }
    }
}
