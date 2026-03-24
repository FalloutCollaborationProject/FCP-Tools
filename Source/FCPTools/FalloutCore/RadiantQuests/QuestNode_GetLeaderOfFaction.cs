using RimWorld.QuestGen;

namespace FCP.Core.RadiantQuests;

public class QuestNode_GetLeaderOfFaction : QuestNode
{
    [NoTranslate]
    public SlateRef<string> storeAs;

    public SlateRef<FactionDef> factionDef = null;
    public SlateRef<Faction> faction = null;

    public SlateRef<bool> factionMustBePermanent = true;

    protected override bool TestRunInt(Slate slate)
    {
        FCPLog.Verbose("Testing");
        FCPLog.Verbose("factionDef is null: ");
        FCPLog.Verbose(factionDef == null);
        FCPLog.Verbose("faction is null: ");
        FCPLog.Verbose(faction == null);
        if (factionDef != null || faction != null)
        {
            SetVars(QuestGen.slate);
            return true;
        }
        return false;
    }

    private bool TryFindFaction(out Faction faction, Slate slate)
    {
        FCPLog.Verbose(factionDef.GetValue(slate).defName);
        return Find.FactionManager.GetFactions().Where(c => c.def.defName == factionDef.GetValue(slate).defName).TryRandomElement(out faction);
    }

    protected override void RunInt()
    {
        SetVars(QuestGen.slate);
    }

    private void SetVars(Slate slate)
    {
        FCPLog.Verbose("trying to get faction");
        Faction lfaction = null;
        if (faction != null)
        {
            lfaction = faction.GetValue(slate);
        }
        else
        {
            TryFindFaction(out lfaction, slate);
        }

        FCPLog.Verbose(lfaction.def.defName);

        Pawn pawn = GetFactionLeader(lfaction);
        FCPLog.Verbose(pawn.Label);
/*            QuestPart_InvolvedFactions questPart_InvolvedFactions = new QuestPart_InvolvedFactions();
            FCPLog.Verbose(1);
            questPart_InvolvedFactions.factions.Add(lfaction);
            FCPLog.Verbose(2);
            QuestGen.quest.AddPart(questPart_InvolvedFactions);*/
        FCPLog.Verbose(3);
        QuestGen.slate.Set(storeAs.GetValue(slate), pawn);
        //Log.Message(4);
        //Log.Message(pawn.Label);
    }
    private Pawn GetFactionLeader(Faction faction)
    {
        FCPLog.Verbose(faction.def.label);
        FCPLog.Verbose(faction.leader.LabelCap);
        if (faction != null)
        {
            FCPLog.Verbose("Faction is NOT null");
            return faction.leader;
        }
        return null;
    }

}