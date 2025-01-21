using RimWorld;
using RimWorld.BaseGen;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Tilemaps;
using Verse;

namespace FCP_CaravanIncidents
{
    public class QuestNode_EnterCaravanIntoMap : QuestNode
    {
        [NoTranslate]
        public SlateRef<string> inSignal;
        public SlateRef<int> tile;
        protected override bool TestRunInt(Slate slate)
        {

            if(!tile.TryGetValue(slate, out _))
            {
                return false;
            }
 
            return true;
        }
        protected override void RunInt()
        {
            Log.Message("Running");
            Slate slate = QuestGen.slate;
            Log.Message("Slate");

            int t = tile.GetValue(slate);
            Caravan car = slate.Get<Caravan>("caravan");
            string text = QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate)) ?? QuestGen.slate.Get<string>("inSignal");
            QuestPart_EnterCaravanIntoMap questPart_EnterCaravanIntoMap = new QuestPart_EnterCaravanIntoMap();
            questPart_EnterCaravanIntoMap.caravan = car;
            questPart_EnterCaravanIntoMap.inSignal = text;
            questPart_EnterCaravanIntoMap.tile = t;
            QuestGen.quest.AddPart(questPart_EnterCaravanIntoMap);


            
        }


    }
}
