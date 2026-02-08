using UnityEngine;

namespace FCP.Core;

public class DebugSettings : SettingsTab
{
    public override string TabName => "FCP_Settings_Debug".Translate();
    public override string TabToolTip => "FCP_Settings_Debug_tt".Translate();

    public bool verboseLogging;

    public override void DoTabWindowContents(Rect tabRect)
    {
        var list = new Listing_Standard();
        list.Begin(tabRect);
        
        // Stimpacks
        Text.Font = GameFont.Medium;
        list.Label("FCP_Settings_Debug_Logging".Translate());
        Text.Font = GameFont.Small;
        list.GapLine();
        list.CheckboxLabeled("FCP_Settings_Debug_VerboseLogging".Translate(), ref verboseLogging);
        
        list.End();
    }

    public override void ExposeData()
    {
        Scribe_Values.Look(ref verboseLogging, nameof(verboseLogging), defaultValue: false);
    }
}