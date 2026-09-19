using UnityEngine;

namespace FCP.Core;

public class InstituteSettings : SettingsTab
{
    public override string TabName => "FCP_Settings_Institute".Translate();
    public override string TabToolTip => "FCP_Settings_Institute_tt".Translate();
    public override bool Enabled => ModsConfig.IsActive("Rick.FCP.Institute");

    public bool synthReplacementEnabled = true;
    public float synthReplacementChance = 0.005f;

    public bool relocationOfferEnabled = true;
    public float relocationRewardMin = 300f;
    public float relocationRewardMax = 600f;
    public int relocationGoodwillGain = 10;

    public override void DoTabWindowContents(Rect tabRect)
    {
        GUI.color = TerminalColors.PrimaryColor;
        var list = new Listing_Standard();
        list.Begin(tabRect);

        Text.Font = GameFont.Medium;
        GUI.color = TerminalColors.HighlightColor;
        list.Label("FCP_Settings_Institute_Replacement".Translate());
        GUI.color = TerminalColors.PrimaryColor;
        Text.Font = GameFont.Small;
        list.GapLine();
        list.CheckboxLabeled("FCP_Settings_Institute_ReplacementEnabled".Translate(), ref synthReplacementEnabled,
            "FCP_Settings_Institute_ReplacementEnabled_Desc".Translate());
        if (synthReplacementEnabled)
        {
            synthReplacementChance = list.SliderLabeled(
                "FCP_Settings_Institute_ReplacementChance".Translate(synthReplacementChance.ToStringPercent()),
                synthReplacementChance, 0f, 0.05f, tooltip: "FCP_Settings_Institute_ReplacementChance_Desc".Translate());
        }

        list.Gap();
        Text.Font = GameFont.Medium;
        GUI.color = TerminalColors.HighlightColor;
        list.Label("FCP_Settings_Institute_Relocation".Translate());
        GUI.color = TerminalColors.PrimaryColor;
        Text.Font = GameFont.Small;
        list.GapLine();
        list.CheckboxLabeled("FCP_Settings_Institute_RelocationEnabled".Translate(), ref relocationOfferEnabled,
            "FCP_Settings_Institute_RelocationEnabled_Desc".Translate());
        if (relocationOfferEnabled)
        {
            relocationRewardMin = list.SliderLabeled(
                "FCP_Settings_Institute_RelocationRewardMin".Translate(Mathf.RoundToInt(relocationRewardMin)),
                relocationRewardMin, 50f, 1000f);
            relocationRewardMax = Mathf.Max(relocationRewardMin, list.SliderLabeled(
                "FCP_Settings_Institute_RelocationRewardMax".Translate(Mathf.RoundToInt(relocationRewardMax)),
                relocationRewardMax, 50f, 1500f));
            relocationGoodwillGain = Mathf.RoundToInt(list.SliderLabeled(
                "FCP_Settings_Institute_RelocationGoodwill".Translate(relocationGoodwillGain),
                relocationGoodwillGain, 0f, 50f));
        }

        list.End();
        GUI.color = Color.white;
    }

    public override void ExposeData()
    {
        Scribe_Values.Look(ref synthReplacementEnabled, nameof(synthReplacementEnabled), defaultValue: true);
        Scribe_Values.Look(ref synthReplacementChance, nameof(synthReplacementChance), defaultValue: 0.005f);
        Scribe_Values.Look(ref relocationOfferEnabled, nameof(relocationOfferEnabled), defaultValue: true);
        Scribe_Values.Look(ref relocationRewardMin, nameof(relocationRewardMin), defaultValue: 300f);
        Scribe_Values.Look(ref relocationRewardMax, nameof(relocationRewardMax), defaultValue: 600f);
        Scribe_Values.Look(ref relocationGoodwillGain, nameof(relocationGoodwillGain), defaultValue: 10);
    }
}
