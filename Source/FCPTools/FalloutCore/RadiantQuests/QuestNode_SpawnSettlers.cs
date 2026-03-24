using RimWorld.QuestGen;

namespace FCP.Core.RadiantQuests
{
    public class QuestNode_SpawnSettlers : QuestNode
    {

        public SlateRef<string> inSignal;
        public SlateRef<IEnumerable<Pawn>> pawns;
        public SlateRef<int> radius;
        protected override bool TestRunInt(Slate slate)
        {
            FCPLog.Verbose("SpawnSettlers test");
            if (pawns.GetValue(slate) == null)
            {
                FCPLog.Verbose("Pawns are null");
                return false;
            }
            FCPLog.Verbose("Pawns are not null");
            return true;
        }
        protected override void RunInt()
        {
            Slate slate = QuestGen.slate;
            QuestPart_SpawnSettlers questPart = new QuestPart_SpawnSettlers();
            questPart.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate)) ?? QuestGen.slate.Get<string>("inSignal");
            questPart.pawns = pawns.GetValue(slate).ToList();
            questPart.radius = radius.TryGetValue(slate, out int rad) ? rad : 20;
            questPart.mapTile = slate.Get<int>("siteTile");
            QuestGen.quest.AddPart(questPart);
        }


    }
}

