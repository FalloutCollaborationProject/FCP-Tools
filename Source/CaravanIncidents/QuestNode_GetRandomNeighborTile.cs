using RimWorld;
using RimWorld.BaseGen;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace FCP_CaravanIncidents
{
    public class QuestNode_GetRandomNeighborTile : QuestNode
    {
        public SlateRef<int> tile;
        public SlateRef<string> storeAs;

        protected override bool TestRunInt(Slate slate)
        {

            return SetVars(slate);
         
        }
        protected override void RunInt()
        {
            SetVars(QuestGen.slate);
        }

        public bool SetVars(Slate slate)
        {
            int t = tile.GetValue(slate);
            List<int> neighbors = [];
            Find.World.grid.GetTileNeighbors(t, neighbors);
            foreach(int neighbor in neighbors.InRandomOrder())
            {
                if (!Find.World.worldObjects.AnyWorldObjectAt(neighbor) && !Find.World.Impassable(neighbor))
                {
                    slate.Set(storeAs.GetValue(slate), neighbors.RandomElement());
                    return true;
                }
            }
            return false;
        }

    }
}
