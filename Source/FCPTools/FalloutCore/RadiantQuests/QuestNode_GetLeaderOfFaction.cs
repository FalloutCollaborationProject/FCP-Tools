using RimWorld.QuestGen;

namespace FCP.Core.RadiantQuests;

public class QuestNode_GetLeaderOfFaction : QuestNode
{
    [NoTranslate]
    public SlateRef<string> storeAs;

    public SlateRef<FactionDef> factionDef = null;
    public SlateRef<Faction> faction = null;
    public SlateRef<Faction> factionToUse = null;

    public SlateRef<bool> factionMustBePermanent = true;

    protected override bool TestRunInt(Slate slate)
    {
        return factionDef != null || faction != null || factionToUse != null;
    }

    private bool TryFindFaction(out Faction outFaction, Slate slate)
    {
        outFaction = null;
        if (!factionDef.TryGetValue(slate, out FactionDef def) || def == null)
        {
            return false;
        }
        return Find.FactionManager.GetFactions().Where(c => c.def == def).TryRandomElement(out outFaction);
    }

    protected override void RunInt()
    {
        SetVars(QuestGen.slate);
    }

    private void SetVars(Slate slate)
    {
        Faction lfaction = null;
        if (factionToUse != null && factionToUse.TryGetValue(slate, out Faction ftu))
        {
            lfaction = ftu;
        }
        else if (faction != null && faction.TryGetValue(slate, out Faction f))
        {
            lfaction = f;
        }
        else if (factionDef != null)
        {
            TryFindFaction(out lfaction, slate);
        }

        if (lfaction == null)
        {
            FCPLog.Error("Failed to find faction for quest");
            return;
        }

        Pawn pawn = GetFactionLeader(lfaction);
        if (pawn == null)
        {
            FCPLog.Error("Failed to get faction leader");
            return;
        }

        QuestGen.slate.Set(storeAs.GetValue(slate), pawn);
    }
    private Pawn GetFactionLeader(Faction faction)
    {
        if (faction?.leader != null)
        {
            return faction.leader;
        }

        FCPLog.Error($"Faction {faction?.def?.label ?? "null"} has no leader");
        return null;
    }

}