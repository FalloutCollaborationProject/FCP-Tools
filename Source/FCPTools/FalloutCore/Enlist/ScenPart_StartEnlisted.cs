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
            FCPLog.Warning($"ScenPart_StartEnlisted for {factionDef?.defName ?? "null"} skipped: enlisted starts are disabled in mod settings.");
            return;
        }

        if (factionDef == null)
        {
            FCPLog.Warning("ScenPart_StartEnlisted has no factionDef set.");
            return;
        }

        Faction faction = Find.FactionManager.AllFactionsListForReading
            .FirstOrDefault(f => f.def == factionDef && !f.IsPlayer);
        if (faction == null)
        {
            FCPLog.Warning($"ScenPart_StartEnlisted couldn't find a live Faction for {factionDef.defName} - it never got generated in this world, so the scenario's enlisted start silently did nothing.");
            return;
        }

        FactionEnlistOptionsDef optionsDef = enlistOptionsDef ?? faction.GetEnlistOptions().FirstOrDefault();
        if (optionsDef == null)
        {
            FCPLog.Warning($"ScenPart_StartEnlisted for {factionDef.defName} found the faction but has no FactionEnlistOptionsDef to enlist with (set enlistOptionsDef explicitly, or add one to the faction's FactionEnlistOptions extension).");
            return;
        }

        optionsDef.Worker.EnlistTo(faction);
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
