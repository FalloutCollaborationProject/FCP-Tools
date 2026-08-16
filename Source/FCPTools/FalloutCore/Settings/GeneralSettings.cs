using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using RimWorld;

namespace FCP.Core;

public class GeneralSettings : SettingsTab
{
    public override string TabName => "FCP_Settings.General".Translate();
    public override string TabToolTip => "FCP_Settings.General.tt".Translate();

    public TerminalColorTheme terminalColorTheme = TerminalColorTheme.Green;

    public TerminalColorTheme questIconColorTheme = TerminalColorTheme.Green;

    public bool autoStim = true;
    public bool teetotalerAutoStim = false;

    public bool weaponConditionEnabled = true;
    public float weaponDegradationRate = 1f;

    public bool consumableGrenades = true;

    public bool namedSettlementsEnabled = true;
    public bool namedSettlementTradersOnly = true;

    public override void DoTabWindowContents(Rect tabRect)
    {
        var list = new Listing_Standard();
        list.Begin(tabRect);

        Text.Font = GameFont.Medium;
        list.Label("FCP_Settings_Terminal".Translate());
        Text.Font = GameFont.Small;
        list.GapLine();

        if (list.ButtonTextLabeled("FCP_Settings_Terminal_ColorTheme".Translate(),
            ("FCP_Settings_Terminal_ColorTheme_" + terminalColorTheme).Translate()))
        {
            var options = new List<FloatMenuOption>();
            foreach (TerminalColorTheme theme in Enum.GetValues(typeof(TerminalColorTheme)))
            {
                options.Add(new FloatMenuOption(
                    ("FCP_Settings_Terminal_ColorTheme_" + theme).Translate(),
                    () => terminalColorTheme = theme));
            }
            Find.WindowStack.Add(new FloatMenu(options));
        }

        if (list.ButtonTextLabeled("FCP_Settings_QuestIcon_ColorTheme".Translate(),
            ("FCP_Settings_Terminal_ColorTheme_" + questIconColorTheme).Translate()))
        {
            var options = new List<FloatMenuOption>();
            foreach (TerminalColorTheme theme in Enum.GetValues(typeof(TerminalColorTheme)))
            {
                options.Add(new FloatMenuOption(
                    ("FCP_Settings_Terminal_ColorTheme_" + theme).Translate(),
                    () => questIconColorTheme = theme));
            }
            Find.WindowStack.Add(new FloatMenu(options));
        }

        list.Gap();
        Text.Font = GameFont.Medium;
        list.Label("FCP_Settings_Stims".Translate());
        Text.Font = GameFont.Small;
        list.GapLine();
        list.CheckboxLabeled("FCP_Settings_Stims_AutoStim".Translate(), ref autoStim,
            "FCP_Settings_Stims_AutoStim_Desc".Translate());
        list.CheckboxLabeled("FCP_Settings_Stims_TeetotalerAutoStim".Translate(), ref teetotalerAutoStim,
            "FCP_Settings_Stims_TeetotalerAutoStim_Desc".Translate());

        list.Gap();
        Text.Font = GameFont.Medium;
        list.Label("FCP_Settings_WeaponCondition".Translate());
        Text.Font = GameFont.Small;
        list.GapLine();
        list.CheckboxLabeled("FCP_Settings_WeaponCondition_Enabled".Translate(), ref weaponConditionEnabled,
            "FCP_Settings_WeaponCondition_Enabled_Desc".Translate());
        weaponDegradationRate = list.SliderLabeled(
            "FCP_Settings_WeaponCondition_DegradationRate".Translate(weaponDegradationRate.ToStringPercent()),
            weaponDegradationRate, 0f, 3f, tooltip: "FCP_Settings_WeaponCondition_DegradationRate_Desc".Translate());

        list.Gap();
        Text.Font = GameFont.Medium;
        list.Label("FCP_Settings_Grenades".Translate());
        Text.Font = GameFont.Small;
        list.GapLine();
        list.CheckboxLabeled("FCP_Settings_Grenades_Consumable".Translate(), ref consumableGrenades,
            "FCP_Settings_Grenades_Consumable_Desc".Translate());

        list.Gap();
        Text.Font = GameFont.Medium;
        list.Label("FCP_Settings_NamedSettlements".Translate());
        Text.Font = GameFont.Small;
        list.GapLine();
        list.CheckboxLabeled("FCP_Settings_NamedSettlements_Enabled".Translate(), ref namedSettlementsEnabled,
            "FCP_Settings_NamedSettlements_Enabled_Desc".Translate());
        if (namedSettlementsEnabled)
        {
            list.CheckboxLabeled("FCP_Settings_NamedTraders_Enabled".Translate(), ref namedSettlementTradersOnly,
                "FCP_Settings_NamedTraders_Enabled_Desc".Translate());
        }

        list.End();
    }

    public override void ExposeData()
    {
        Scribe_Values.Look(ref terminalColorTheme, nameof(terminalColorTheme), defaultValue: TerminalColorTheme.Green);

        Scribe_Values.Look(ref questIconColorTheme, nameof(questIconColorTheme), defaultValue: TerminalColorTheme.Green);

        Scribe_Values.Look(ref autoStim, nameof(autoStim), defaultValue: true);
        Scribe_Values.Look(ref teetotalerAutoStim, nameof(teetotalerAutoStim), defaultValue: false);

        Scribe_Values.Look(ref weaponConditionEnabled, nameof(weaponConditionEnabled), defaultValue: true);
        Scribe_Values.Look(ref weaponDegradationRate, nameof(weaponDegradationRate), defaultValue: 1f);

        Scribe_Values.Look(ref consumableGrenades, nameof(consumableGrenades), defaultValue: true);

        Scribe_Values.Look(ref namedSettlementsEnabled, nameof(namedSettlementsEnabled), defaultValue: true);
        Scribe_Values.Look(ref namedSettlementTradersOnly, nameof(namedSettlementTradersOnly), defaultValue: true);
    }
}
