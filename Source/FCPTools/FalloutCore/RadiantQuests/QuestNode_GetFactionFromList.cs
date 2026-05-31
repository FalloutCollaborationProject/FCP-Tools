using RimWorld.QuestGen;

namespace FCP.Core.RadiantQuests;

public class QuestNode_GetFactionFromList : QuestNode
{
    [NoTranslate]
    public SlateRef<string> storeAs;

    public SlateRef<IEnumerable<FactionDef>> factionDefs;
    public SlateRef<bool> mustBeNonHostile = true;

    protected override bool TestRunInt(Slate slate)
    {
        if (factionDefs == null) return false;
        
        IEnumerable<FactionDef> defs;
        if (!factionDefs.TryGetValue(slate, out defs) || defs == null) return false;
        
        var allFactions = Find.FactionManager.GetFactions().Where(c => defs.Any(x => x.defName == c.def.defName)).ToList();
        
        bool checkHostile = mustBeNonHostile.GetValue(slate);
        var validFactions = checkHostile ? allFactions.Where(f => !f.HostileTo(Faction.OfPlayer)).ToList() : allFactions;
        
        return validFactions.Any();
    }

    protected override void RunInt()
    {
        SetVars(QuestGen.slate);
    }

    private void SetVars(Slate slate)
    {
        var allFactions = Find.FactionManager.GetFactions()
            .Where(c => factionDefs.GetValue(slate).Any(x => x.defName == c.def.defName));
        
        bool checkHostile = mustBeNonHostile.GetValue(slate);
        var validFactions = checkHostile ? allFactions.Where(f => !f.HostileTo(Faction.OfPlayer)) : allFactions;
        
        if (validFactions.TryRandomElement(out Faction faction))
        {
            slate.Set(storeAs.GetValue(slate), faction);
        }
    }
}