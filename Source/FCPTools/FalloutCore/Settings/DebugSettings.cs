using UnityEngine;

namespace FCP.Core;

public class DebugSettings : SettingsTab
{
    public override string TabName => "FCP_Settings_Debug".Translate();
    public override string TabToolTip => "FCP_Settings_Debug_tt".Translate();

    public bool verboseLogging;

    public override void DoTabWindowContents(Rect tabRect)
    {
        GUI.color = TerminalColors.PrimaryColor;
        var list = new Listing_Standard();
        list.Begin(tabRect);

        Text.Font = GameFont.Medium;
        GUI.color = TerminalColors.HighlightColor;
        list.Label("FCP_Settings_Debug_Logging".Translate());
        GUI.color = TerminalColors.PrimaryColor;
        Text.Font = GameFont.Small;
        list.GapLine();
        list.CheckboxLabeled("FCP_Settings_Debug_VerboseLogging".Translate(), ref verboseLogging);

        list.End();
        GUI.color = Color.white;
    }

    public override void ExposeData()
    {
        Scribe_Values.Look(ref verboseLogging, nameof(verboseLogging), defaultValue: false);
    }
}