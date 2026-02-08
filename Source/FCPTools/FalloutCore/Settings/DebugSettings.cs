using UnityEngine;

namespace FCP.Core;

public class DebugSettings : SettingsTab
{
    public override string TabName => "FCP_Settings.Debug".Translate();
    public override string TabToolTip => "FCP_Settings.Debug.tt".Translate();

    public bool verboseLogging;

    public override void DoTabWindowContents(Rect tabRect)
    {
        var list = new Listing_Standard();
        list.Begin(tabRect);
        
        list.Label("Logging");
        list.CheckboxLabeled("Verbose Logging", ref verboseLogging);
        
        list.End();
    }

    public override void ExposeData()
    {
        Scribe_Values.Look(ref verboseLogging, nameof(verboseLogging), defaultValue: false);
    }
}