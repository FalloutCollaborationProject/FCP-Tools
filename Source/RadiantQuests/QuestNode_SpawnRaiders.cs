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
    public class QuestNode_SpawnRaiders : QuestNode
    {

        public SlateRef<string> inSignal;
        public SlateRef<IEnumerable<Pawn>> pawns;
        public SlateRef<int> radius;
        public SlateRef<bool> spawnOnEdge;
        protected override bool TestRunInt(Slate slate)
        {
            Log.Message("SpawnRaiders test");
            if (pawns.GetValue(slate) == null)
            {
                Log.Message("Pawns are null");
                return false;
            }
            Log.Message("Pawns are not null");
            return true;
        }
        protected override void RunInt()
        {
            Slate slate = QuestGen.slate;
            QuestPart_SpawnRaiders questPart = new QuestPart_SpawnRaiders();
            questPart.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate)) ?? QuestGen.slate.Get<string>("inSignal");
            questPart.pawns = pawns.GetValue(slate).ToList();
            questPart.radius = radius.TryGetValue(slate, out int rad) ? rad : 20;
            questPart.mapTile = slate.Get<int>("siteTile");
            questPart.spawnOnEdge = spawnOnEdge.GetValue(slate);
            QuestGen.quest.AddPart(questPart);
        }


    }
}
