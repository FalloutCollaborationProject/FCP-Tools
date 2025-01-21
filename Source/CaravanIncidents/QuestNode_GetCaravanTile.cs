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
    public class QuestNode_GetCaravanTile : QuestNode
    {
        public SlateRef<string> storeAs;

        protected override bool TestRunInt(Slate slate)
        {
            if (!slate.TryGet<Caravan>("caravan", out _))
            {
                return false;

            }
            SetVars(slate);
            return true;
         
        }
        protected override void RunInt()
        {
            SetVars(QuestGen.slate);
        }

        public void SetVars(Slate slate)
        {
            Caravan caravan = slate.Get<Caravan>("caravan");
            Log.Message("Caravan tile = " + caravan.GetTileCurrentlyOver());
            slate.Set(storeAs.GetValue(slate), caravan.GetTileCurrentlyOver());
        }

    }
}
