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

    // Terminal UI
    public TerminalColorTheme terminalColorTheme = TerminalColorTheme.Green;
    
    // Quest UI
    public TerminalColorTheme questIconColorTheme = TerminalColorTheme.Green;

    // Stimpacks
    public bool autoStim = true;
    public bool teetotalerAutoStim = false;

    // Weapon Condition
    public bool weaponConditionEnabled = true;

    // Grenades
    public bool consumableGrenades = true;

    public override void DoTabWindowContents(Rect tabRect)
    {
        var list = new Listing_Standard();
        list.Begin(tabRect);

        // Terminal UI
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

        // Stimpacks
        list.Gap();
        Text.Font = GameFont.Medium;
        list.Label("FCP_Settings_Stims".Translate());
        Text.Font = GameFont.Small;
        list.GapLine();
        list.CheckboxLabeled("FCP_Settings_Stims_AutoStim".Translate(), ref autoStim,
            "FCP_Settings_Stims_AutoStim_Desc".Translate());
        list.CheckboxLabeled("FCP_Settings_Stims_TeetotalerAutoStim".Translate(), ref teetotalerAutoStim,
            "FCP_Settings_Stims_TeetotalerAutoStim_Desc".Translate());

        // Weapon Condition
        list.Gap();
        Text.Font = GameFont.Medium;
        list.Label("FCP_Settings_WeaponCondition".Translate());
        Text.Font = GameFont.Small;
        list.GapLine();
        list.CheckboxLabeled("FCP_Settings_WeaponCondition_Enabled".Translate(), ref weaponConditionEnabled,
            "FCP_Settings_WeaponCondition_Enabled_Desc".Translate());

        // Grenades
        list.Gap();
        Text.Font = GameFont.Medium;
        list.Label("FCP_Settings_Grenades".Translate());
        Text.Font = GameFont.Small;
        list.GapLine();
        list.CheckboxLabeled("FCP_Settings_Grenades_Consumable".Translate(), ref consumableGrenades,
            "FCP_Settings_Grenades_Consumable_Desc".Translate());

        list.End();
    }

    public override void ExposeData()
    {
        // Terminal UI
        Scribe_Values.Look(ref terminalColorTheme, nameof(terminalColorTheme), defaultValue: TerminalColorTheme.Green);
        
        // Quest UI
        Scribe_Values.Look(ref questIconColorTheme, nameof(questIconColorTheme), defaultValue: TerminalColorTheme.Green);

        // Stimpacks
        Scribe_Values.Look(ref autoStim, nameof(autoStim), defaultValue: true);
        Scribe_Values.Look(ref teetotalerAutoStim, nameof(teetotalerAutoStim), defaultValue: false);

        // Weapon Condition
        Scribe_Values.Look(ref weaponConditionEnabled, nameof(weaponConditionEnabled), defaultValue: true);

        // Grenades
        Scribe_Values.Look(ref consumableGrenades, nameof(consumableGrenades), defaultValue: true);
    }
}