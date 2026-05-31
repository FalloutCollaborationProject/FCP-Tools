using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;

namespace FCP.Core.RadiantQuests;

public class QuestNode_SpawnPawns : QuestNode
{
    public SlateRef<string> inSignal;
    public SlateRef<IEnumerable<Pawn>> pawns;
    public SlateRef<MapParent> mapParent;

    protected override bool TestRunInt(Slate slate) => true;

    protected override void RunInt()
    {
        Slate slate = QuestGen.slate;
        
        var pawnList = pawns.GetValue(slate);
        if (pawnList == null) return;
        
        var pawnArray = pawnList.ToList();
        var mapParentValue = mapParent.GetValue(slate);
        
        QuestPart_SpawnPawns part = new QuestPart_SpawnPawns();
        part.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate)) ?? slate.Get<string>("inSignal");
        part.pawns = pawnArray;
        part.mapParent = mapParentValue;
        QuestGen.quest.AddPart(part);
    }
}
