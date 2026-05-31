using RimWorld;
using RimWorld.QuestGen;
using Verse;

namespace FCP.Core.RadiantQuests;

public class QuestNode_GetFactionLeader : QuestNode
{
    public SlateRef<Faction> faction;
    public SlateRef<string> storeAs;

    protected override bool TestRunInt(Slate slate)
    {
        Faction fac = faction.GetValue(slate);
        bool result = fac?.leader != null && !fac.leader.Dead;
        Log.Message($"[VertibirdCrash] QuestNode_GetFactionLeader.TestRunInt: faction={fac?.Name.ToStringSafe() ?? "null"}, leader={fac?.leader?.Name.ToStringSafe() ?? "null"}, result={result}");
        return result;
    }

    protected override void RunInt()
    {
        Slate slate = QuestGen.slate;
        Faction fac = faction.GetValue(slate);
        if (fac?.leader != null && !fac.leader.Dead)
        {
            slate.Set(storeAs.GetValue(slate), fac.leader);
        }
    }
}
