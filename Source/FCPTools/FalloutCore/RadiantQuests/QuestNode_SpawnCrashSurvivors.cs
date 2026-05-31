using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;

namespace FCP.Core.RadiantQuests;

public class QuestNode_SpawnCrashSurvivors : QuestNode
{
    public SlateRef<string> inSignal;
    public SlateRef<IEnumerable<Pawn>> pawns;

    protected override bool TestRunInt(Slate slate)
    {
        return true;
    }

    protected override void RunInt()
    {
        Slate slate = QuestGen.slate;
        IEnumerable<Pawn> pawnList = pawns.GetValue(slate);
        if (pawnList == null) return;

        QuestPart_SpawnCrashSurvivors part = new QuestPart_SpawnCrashSurvivors();
        part.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate)) ?? slate.Get<string>("inSignal");
        part.pawns = pawnList.ToList();
        part.mapParent = slate.Get<MapParent>("site");
        QuestGen.quest.AddPart(part);
    }
}
