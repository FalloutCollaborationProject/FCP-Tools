using UnityEngine;
using Verse;

namespace FCP.Core;

public class ScenarioSettings : SettingsTab
{
    public override string TabName => "FCP_Settings.Scenarios".Translate();
    public override string TabToolTip => "FCP_Settings.Scenarios.tt".Translate();

    public bool enableEnlistedStart = true;
    public int startingYear = 2287;

    [Unsaved] private string startingYearBuffer;

    private static Color DimPrimary => new Color(TerminalColors.PrimaryColor.r, TerminalColors.PrimaryColor.g, TerminalColors.PrimaryColor.b, 0.5f);

    public override void DoTabWindowContents(Rect tabRect)
    {
        GUI.color = TerminalColors.PrimaryColor;
        var list = new Listing_Standard();
        list.Begin(tabRect);

        Text.Font = GameFont.Medium;
        GUI.color = TerminalColors.HighlightColor;
        list.Label("FCP_Settings_Scenarios_Enlistment".Translate());
        GUI.color = TerminalColors.PrimaryColor;
        Text.Font = GameFont.Small;
        list.GapLine();
        list.CheckboxLabeled("FCP_Settings_Scenarios_EnableEnlistedStart".Translate(), ref enableEnlistedStart,
            "FCP_Settings_Scenarios_EnableEnlistedStart_Desc".Translate());

        list.Gap();
        Text.Font = GameFont.Medium;
        GUI.color = TerminalColors.HighlightColor;
        list.Label("FCP_Settings_Scenarios_Calendar".Translate());
        GUI.color = TerminalColors.PrimaryColor;
        Text.Font = GameFont.Small;
        list.GapLine();
        startingYearBuffer ??= startingYear.ToString();
        list.TextFieldNumericLabeled("FCP_Settings_Scenarios_StartingYear".Translate(), ref startingYear,
            ref startingYearBuffer, 1f, 9999f);
        GUI.color = DimPrimary;
        list.Label("FCP_Settings_Scenarios_StartingYear_Desc".Translate());
        GUI.color = TerminalColors.PrimaryColor;

        list.End();
        GUI.color = Color.white;
    }

    public override void ExposeData()
    {
        Scribe_Values.Look(ref enableEnlistedStart, nameof(enableEnlistedStart), defaultValue: true);
        Scribe_Values.Look(ref startingYear, nameof(startingYear), defaultValue: 2287);
    }
}
