using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;

namespace FCP.Core.RadiantQuests;

public class QuestNode_SpawnCorpses : QuestNode
{
    [NoTranslate]
    public SlateRef<string> inSignal;
    public SlateRef<List<Pawn>> pawns;
    public SlateRef<int> radius = 10;

    protected override bool TestRunInt(Slate slate)
    {
        return pawns != null;
    }

    protected override void RunInt()
    {
        Slate slate = QuestGen.slate;
        QuestPart_SpawnCorpses part = new QuestPart_SpawnCorpses
        {
            inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate)),
            pawns = pawns.GetValue(slate),
            radius = radius.GetValue(slate),
            mapTile = slate.Get<PlanetTile>("siteTile")
        };
        QuestGen.quest.AddPart(part);
    }
}
