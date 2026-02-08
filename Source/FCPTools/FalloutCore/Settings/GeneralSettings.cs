using UnityEngine;

namespace FCP.Core;

public class GeneralSettings : SettingsTab
{
    public override string TabName => "FCP_Settings.General".Translate();
    public override string TabToolTip => "FCP_Settings.General.tt".Translate();

    // Stimpacks
    public bool autoStim = true;
    public bool teetotalerAutoStim = false;

    public override void DoTabWindowContents(Rect tabRect)
    {
        var list = new Listing_Standard();
        list.Begin(tabRect);

        // Stimpacks
        Text.Font = GameFont.Medium;
        list.Label("FCP_Settings_Stims".Translate());
        Text.Font = GameFont.Small;
        list.GapLine();
        list.CheckboxLabeled("FCP_Settings_Stims_AutoStim".Translate(), ref autoStim,
            "FCP_Settings_Stims_AutoStim_Desc".Translate());
        list.CheckboxLabeled("FCP_Settings_Stims_TeetotalerAutoStim".Translate(), ref teetotalerAutoStim,
            "FCP_Settings_Stims_TeetotalerAutoStim_Desc".Translate());

        list.End();
    }

    public override void ExposeData()
    {
        // Stimpacks
        Scribe_Values.Look(ref autoStim, nameof(autoStim), defaultValue: true);
        Scribe_Values.Look(ref teetotalerAutoStim, nameof(teetotalerAutoStim), defaultValue: false);
    }
}