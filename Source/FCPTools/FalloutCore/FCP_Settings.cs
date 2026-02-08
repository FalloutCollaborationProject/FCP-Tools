using UnityEngine;
using System.Reflection;
using System.Collections;

namespace FCP.Core;

public class FCP_Settings : ModSettings
{
    public float FlatHitChanceBoost = 0.2f;
    public int CooldownTicks = 600;
    public bool EnableSlowDownTime = true;
    public bool EnableZoom = true;

    public Dictionary<string, float> MultiplierLookup = new Dictionary<string, float>();
    public Vector2 scrollPosition = Vector2.zero;
    public int ZoomTimeout = 150;

    public void ResetMultipliers()
    {
        MultiplierLookup = new Dictionary<string, float>
        {
            { "Leg", 1.4f },
            { "Eye", 0.6f },
            { "Shoulder", 0.9f },
            { "Arm", 1.3f },
            { "Hand", 0.8f },
            { "Head", 0.7f },
            { "Lung", 0.95f },
            { "Torso", 1.5f },
            { "Heart", 0.8f }
        };

        PopulateMissingMultipliers();
    }

    public void PopulateMissingMultipliers()
    {
        if (MultiplierLookup.Count() >= DefDatabase<BodyPartDef>.AllDefs.Count())
        {
            return;
        }

        IEnumerable<BodyPartDef> defs = DefDatabase<BodyPartDef>.AllDefs.Where(def => !MultiplierLookup.Keys.Contains(def.defName));

        foreach (BodyPartDef bodyPartDef in defs)
        {
            MultiplierLookup[bodyPartDef.defName] = 1.0f;
        }
    }

    public void DoWindowContents(Rect wrect)
    {
        PopulateMissingMultipliers();
        int multiplierHeight = MultiplierLookup.Count * 56;
        
        // Calculate enlist count dynamically
        int enlistCount = 0;
        var enlistModTypeForCount = Type.GetType("FCP.Enlist.EnlistMod, FCP_Enlist");
        if (enlistModTypeForCount != null)
        {
            var settingsField = enlistModTypeForCount.GetField("settings", BindingFlags.Public | BindingFlags.Static);
            if (settingsField != null)
            {
                var enlistSettings = settingsField.GetValue(null);
                if (enlistSettings != null)
                {
                    var enlistStatesField = enlistSettings.GetType().GetField("enlistStates", BindingFlags.Public | BindingFlags.Instance);
                    if (enlistStatesField != null)
                    {
                        var enlistStates = enlistStatesField.GetValue(enlistSettings) as IDictionary;
                        enlistCount = enlistStates?.Count ?? 0;
                    }
                }
            }
        }
        
        int restHeight = 248 + 64 + 200 + (enlistCount * 24); // Added space for new settings sections
        float scrollViewHeight = multiplierHeight + restHeight;
        Rect viewRect = new Rect(0, 0, wrect.width - 20, scrollViewHeight);
        scrollPosition = GUI.BeginScrollView(new Rect(0, 50, wrect.width, wrect.height - 50), scrollPosition, viewRect);
        Listing_Standard options = new Listing_Standard();
        options.Begin(viewRect);
        try
        {
            // === Stims Settings ===
            options.GapLine();
            Text.Font = GameFont.Medium;
            options.Label("FCP_Settings_Stimpaks".Translate());
            Text.Font = GameFont.Small;
            
            // Use reflection to access StimPacks.ModConfig.ConfigUI.Config if available
            var stimConfigType = Type.GetType("StimPacks.ModConfig.ConfigUI, FCP_StimPacks");
            if (stimConfigType != null)
            {
                var configField = stimConfigType.GetField("Config", BindingFlags.Public | BindingFlags.Static);
                if (configField != null)
                {
                    var config = configField.GetValue(null);
                    if (config != null)
                    {
                        var autoStimField = config.GetType().GetField("AutoStim", BindingFlags.Public | BindingFlags.Instance);
                        var teetotalerField = config.GetType().GetField("TeetotalerAutoStim", BindingFlags.Public | BindingFlags.Instance);
                        
                        if (autoStimField != null)
                        {
                            bool autoStim = (bool)autoStimField.GetValue(config);
                            options.CheckboxLabeled("AutoStim".Translate(), ref autoStim, "AutoStimDesc".Translate());
                            autoStimField.SetValue(config, autoStim);
                        }
                        
                        if (teetotalerField != null)
                        {
                            bool teetotalerAutoStim = (bool)teetotalerField.GetValue(config);
                            options.CheckboxLabeled("TeetotalerAutoStim".Translate(), ref teetotalerAutoStim, "TeetotalerAutoStimDesc".Translate());
                            teetotalerField.SetValue(config, teetotalerAutoStim);
                        }
                    }
                }
            }
            options.Gap();

            // === Temperature Apparel Preference Settings ===
            options.GapLine();
            Text.Font = GameFont.Medium;
            options.Label("FCP_Settings_TemperatureApparelPreference".Translate());
            Text.Font = GameFont.Small;
            
            // Use reflection to access FCP.Core.TemperatureApparelPreference.Mod
            var tempApparelModType = Type.GetType("FCP.Core.TemperatureApparelPreference.Mod, FCP_Core");
            if (tempApparelModType != null)
            {
                var instanceProp = tempApparelModType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
                if (instanceProp != null)
                {
                    var instance = instanceProp.GetValue(null);
                    if (instance != null)
                    {
                        var settingsProp = instance.GetType().GetProperty("Settings", BindingFlags.Public | BindingFlags.Instance);
                        if (settingsProp != null)
                        {
                            var settings = settingsProp.GetValue(instance);
                            if (settings != null)
                            {
                                var verboseField = settings.GetType().GetField("verboseLogging", BindingFlags.Public | BindingFlags.Instance);
                                if (verboseField != null)
                                {
                                    bool verboseLogging = (bool)verboseField.GetValue(settings);
                                    options.CheckboxLabeled(
                                        "FCP_Settings_VerboseLogging".Translate(),
                                        ref verboseLogging,
                                        "FCP_Settings_VerboseLoggingDesc".Translate()
                                    );
                                    verboseField.SetValue(settings, verboseLogging);
                                }
                            }
                        }
                    }
                }
            }
            options.Gap();

            // === Enlistment Settings ===
            options.GapLine();
            Text.Font = GameFont.Medium;
            options.Label("FCP_Settings_EnlistmentOptions".Translate());
            Text.Font = GameFont.Small;
            
            // Use reflection to access FCP.Enlist.EnlistMod.settings
            var enlistModType = Type.GetType("FCP.Enlist.EnlistMod, FCP_Enlist");
            if (enlistModType != null)
            {
                var settingsField = enlistModType.GetField("settings", BindingFlags.Public | BindingFlags.Static);
                if (settingsField != null)
                {
                    var enlistSettings = settingsField.GetValue(null);
                    if (enlistSettings != null)
                    {
                        var enlistStatesField = enlistSettings.GetType().GetField("enlistStates", BindingFlags.Public | BindingFlags.Instance);
                        if (enlistStatesField != null)
                        {
                            var enlistStates = enlistStatesField.GetValue(enlistSettings) as IDictionary;
                            if (enlistStates != null && enlistStates.Count > 0)
                            {
                                var keys = enlistStates.Keys.Cast<string>().OrderByDescending(x => x).ToList();
                                options.Label("RH.ActiveEnlistOptions".Translate());
                                foreach (var key in keys)
                                {
                                    bool value = (bool)enlistStates[key];
                                    options.CheckboxLabeled(key, ref value);
                                    enlistStates[key] = value;
                                }
                            }
                        }
                    }
                }
            }
            options.Gap();

            // === VATS Settings ===
            options.GapLine();
            Text.Font = GameFont.Medium;
            options.Label("FCP_Settings_VATS".Translate());
            Text.Font = GameFont.Small;
            
            //30
            if (options.ButtonText("FCP_VATS_Settings_Reset".Translate()))
            {
                EnableSlowDownTime = true;
                EnableZoom = true;
                ZoomTimeout = 150;
                CooldownTicks = 600;
                FlatHitChanceBoost = 0.2f;
                ResetMultipliers();
            }

            //12
            options.Gap();

            //22
            options.Label("FCP_VATS_Settings_FlatHitChanceBoost".Translate(FlatHitChanceBoost.ToString("F5")), tooltip: "FCP_VATS_Settings_FlatHitChanceBoost_Mouseover".Translate());
            //30
            FlatHitChanceBoost = options.Slider(FlatHitChanceBoost, 0f, 1f);
            //12
            options.Gap();

            //30
            options.CheckboxLabeled("FCP_VATS_Settings_EnableSlowDownTime".Translate(), ref EnableSlowDownTime);
            //12
            options.Gap();
            //30

            options.CheckboxLabeled("FCP_VATS_Settings_EnableZoom".Translate(), ref EnableZoom);
            //12
            options.Gap();

            //22
            options.Label("FCP_VATS_Settings_ZoomTimeout".Translate(ZoomTimeout));
            //22
            options.IntAdjuster(ref ZoomTimeout, 1, 60);
            //12
            options.Gap();

            //22
            options.Label("FCP_VATS_Settings_CooldownTicks".Translate(CooldownTicks));
            //22
            options.IntAdjuster(ref CooldownTicks, 1, 60);
            //12
            options.Gap();

            // Body Part Multipliers
            options.GapLine();
            //22
            options.Label("FCP_VATS_Settings_BodyPartMultipliers".Translate());

            foreach (string defName in MultiplierLookup.Keys.ToList())
            {
                BodyPartDef def = DefDatabase<BodyPartDef>.GetNamed(defName);
                if (def == null)
                {
                    continue;
                }

                string label = def.LabelCap;
                options.Label($"[{MultiplierLookup[defName]:F5}] {label} - {def.description}");
                MultiplierLookup[defName] = options.Slider(MultiplierLookup[defName], 0.01f, 10f);
                options.Gap();
            }
        }
        finally
        {
            options.End();
            GUI.EndScrollView();
        }
    }

    public override void ExposeData()
    {
        if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs)
        {
            ResetMultipliers();
        }

        Scribe_Values.Look(ref EnableSlowDownTime, "EnableSlowDownTime", true);
        Scribe_Values.Look(ref EnableZoom, "EnableZoom", true);
        Scribe_Values.Look(ref ZoomTimeout, "ZoomTimeout", 150);
        Scribe_Values.Look(ref CooldownTicks, "CooldownTicks", 150);
        Scribe_Values.Look(ref FlatHitChanceBoost, "FlatHitChanceBoost", 0.15f);
        Scribe_Collections.Look(ref MultiplierLookup, "MultiplierLookup", LookMode.Value, LookMode.Value);
    }
}
