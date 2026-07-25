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

    public override void DoTabWindowContents(Rect tabRect)
    {
        var list = new Listing_Standard();
        list.Begin(tabRect);

        Text.Font = GameFont.Medium;
        list.Label("FCP_Settings_Scenarios_Enlistment".Translate());
        Text.Font = GameFont.Small;
        list.GapLine();
        list.CheckboxLabeled("FCP_Settings_Scenarios_EnableEnlistedStart".Translate(), ref enableEnlistedStart,
            "FCP_Settings_Scenarios_EnableEnlistedStart_Desc".Translate());

        list.Gap();
        Text.Font = GameFont.Medium;
        list.Label("FCP_Settings_Scenarios_Calendar".Translate());
        Text.Font = GameFont.Small;
        list.GapLine();
        startingYearBuffer ??= startingYear.ToString();
        list.TextFieldNumericLabeled("FCP_Settings_Scenarios_StartingYear".Translate(), ref startingYear,
            ref startingYearBuffer, 1f, 9999f);
        GUI.color = Color.gray;
        list.Label("FCP_Settings_Scenarios_StartingYear_Desc".Translate());
        GUI.color = Color.white;

        list.End();
    }

    public override void ExposeData()
    {
        Scribe_Values.Look(ref enableEnlistedStart, nameof(enableEnlistedStart), defaultValue: true);
        Scribe_Values.Look(ref startingYear, nameof(startingYear), defaultValue: 2287);
    }
}
