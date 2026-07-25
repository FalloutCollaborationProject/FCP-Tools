using System.Linq;
using FCP.Core;
using RimWorld;
using Verse;

namespace FCP.Enlist;

public class ScenPart_StartEnlisted : ScenPart
{
    public FactionDef factionDef;
    public FactionEnlistOptionsDef enlistOptionsDef;

    public override void PostGameStart()
    {
        base.PostGameStart();

        if (FCPCoreMod.SettingsTab<ScenarioSettings>() is { enableEnlistedStart: false })
        {
            return;
        }

        if (factionDef == null)
        {
            return;
        }

        Faction faction = Find.FactionManager.AllFactionsListForReading
            .FirstOrDefault(f => f.def == factionDef && !f.IsPlayer);
        if (faction == null)
        {
            return;
        }

        FactionEnlistOptionsDef optionsDef = enlistOptionsDef ?? faction.GetEnlistOptions().FirstOrDefault();
        optionsDef?.Worker.EnlistTo(faction);
    }

    public override string Summary(Scenario scen)
    {
        return "FCP_ScenPart_StartEnlisted".Translate(factionDef?.label ?? "?");
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Defs.Look(ref factionDef, "factionDef");
        Scribe_Defs.Look(ref enlistOptionsDef, "enlistOptionsDef");
    }
}
