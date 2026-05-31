using RimWorld.QuestGen;

namespace FCP.Core.RadiantQuests;

public class QuestNode_SpawnRaiders : QuestNode
{

    public SlateRef<string> inSignal;
    public SlateRef<IEnumerable<Pawn>> pawns;
    public SlateRef<int> radius;
    public SlateRef<bool> spawnOnEdge;
    protected override bool TestRunInt(Slate slate)
    {
        return pawns.GetValue(slate) != null;
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