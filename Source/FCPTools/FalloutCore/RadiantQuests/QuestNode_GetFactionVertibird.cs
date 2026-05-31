using RimWorld.QuestGen;
using Verse;

namespace FCP.Core.RadiantQuests;

public class QuestNode_GetFactionVertibird : QuestNode
{
    public SlateRef<Faction> faction;
    public SlateRef<string> storeAs;

    protected override bool TestRunInt(Slate slate)
    {
        return true;
    }

    protected override void RunInt()
    {
        Slate slate = QuestGen.slate;
        Faction fac = faction.GetValue(slate);
        if (fac?.def == null) return;

        FactionExtension_Vertibird ext = fac.def.GetModExtension<FactionExtension_Vertibird>();
        if (ext?.crashedVertibird != null)
        {
            slate.Set(storeAs.GetValue(slate), ext.crashedVertibird);
        }
    }
}
