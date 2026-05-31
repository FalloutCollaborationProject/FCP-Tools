using RimWorld.QuestGen;
using Verse;

namespace FCP.Core.RadiantQuests;

public class QuestNode_DeliverPawns : QuestNode
{
    public SlateRef<string> inSignal;
    public SlateRef<IEnumerable<Pawn>> pawns;
    public SlateRef<string> outSignalSuccess;
    public SlateRef<string> outSignalFailed;
    public SlateRef<bool?> allowDead;

    protected override bool TestRunInt(Slate slate) => true;

    protected override void RunInt()
    {
        Slate slate = QuestGen.slate;
        
        var pawnList = pawns.GetValue(slate);
        if (pawnList == null) return;
        
        QuestPart_DeliverPawns part = new QuestPart_DeliverPawns
        {
            inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate)) ?? slate.Get<string>("inSignal"),
            pawnsToDeliver = pawnList.ToList(),
            outSignalSuccess = outSignalSuccess.GetValue(slate),
            outSignalFailed = outSignalFailed.GetValue(slate),
            allowDead = allowDead.GetValue(slate) ?? false
        };
        
        QuestGen.quest.AddPart(part);
    }
}
