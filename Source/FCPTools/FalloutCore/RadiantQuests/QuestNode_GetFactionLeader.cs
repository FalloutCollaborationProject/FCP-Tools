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
        return fac?.leader != null && !fac.leader.Dead;
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
