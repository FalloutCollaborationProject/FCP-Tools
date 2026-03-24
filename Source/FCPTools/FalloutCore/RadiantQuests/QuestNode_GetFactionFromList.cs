using RimWorld.QuestGen;

namespace FCP.Core.RadiantQuests;

public class QuestNode_GetFactionFromList : QuestNode
{
    [NoTranslate]
    public SlateRef<string> storeAs;

    public SlateRef<IEnumerable<FactionDef>> factionDefs;

    protected override bool TestRunInt(Slate slate)
    {
        if (Find.FactionManager.GetFactions().Any(c => factionDefs.GetValue(slate).Any(x => x.defName == c.def.defName)))
        {
            FCPLog.Verbose("factions exist");
            SetVars(slate);
            return true;
        }
        FCPLog.Verbose("factions dont exist");
        return false;
    }

    protected override void RunInt()
    {
        SetVars(QuestGen.slate);
    }

    private void SetVars(Slate slate)
    {
        Find.FactionManager.GetFactions().Where(c => factionDefs.GetValue(slate).Any(x => x.defName == c.def.defName)).TryRandomElement(out Faction faction);
        FCPLog.Verbose(faction.def.label);
        slate.Set(storeAs.GetValue(slate), faction);
    }
}