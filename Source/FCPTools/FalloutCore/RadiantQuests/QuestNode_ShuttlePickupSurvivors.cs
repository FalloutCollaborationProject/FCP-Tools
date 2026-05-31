using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;

namespace FCP.Core.RadiantQuests;

public class QuestNode_ShuttlePickupSurvivors : QuestNode
{
    public SlateRef<string> inSignal;
    public SlateRef<IEnumerable<Pawn>> pawns;
    public SlateRef<string> outSignalSuccess;
    public SlateRef<Faction> faction;

    protected override bool TestRunInt(Slate slate)
    {
        return true;
    }

    protected override void RunInt()
    {
        Slate slate = QuestGen.slate;
        IEnumerable<Pawn> pawnList = pawns.GetValue(slate);
        if (pawnList == null) return;

        Faction fac = faction != null ? faction.GetValue(slate) : null;
        if (fac == null && slate.Exists("askerFaction"))
        {
            fac = slate.Get<Faction>("askerFaction");
        }

        QuestPart_ShuttleLeaveWithPawns part = new QuestPart_ShuttleLeaveWithPawns();
        part.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate)) ?? slate.Get<string>("inSignal");
        part.pawns = pawnList.ToList();
        part.mapParent = slate.Get<MapParent>("site");
        part.outSignalSuccess = QuestGenUtility.HardcodedSignalWithQuestID(outSignalSuccess.GetValue(slate));
        part.faction = fac;
        QuestGen.quest.AddPart(part);
    }
}
