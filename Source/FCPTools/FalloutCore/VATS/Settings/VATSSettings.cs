using UnityEngine;

namespace FCP.Core.VATS;

public class VATSSettings : SettingsTab
{
    public override string TabName => "FCP_Settings_VATS".Translate();

    public float flatHitChanceBoost = 0.2f;
    public int cooldownTicks = 600;
    public bool enableSlowDownTime = true;
    public bool enableZoom = true;
    public int zoomTimeout = 150;
    public Dictionary<string, float> multiplierLookup = new();

    private Vector2 scrollPosition = Vector2.zero;

    public override void DoTabWindowContents(Rect wrect)
    {
        PopulateMissingMultipliers();
        int multiplierHeight = multiplierLookup.Count * 56;
        int restHeight = 248 + 64;
        float scrollViewHeight = multiplierHeight + restHeight;
        Rect viewRect = new Rect(wrect.x, wrect.y, wrect.width - 20, scrollViewHeight);
        scrollPosition = GUI.BeginScrollView(new Rect(wrect.x, wrect.y, wrect.width, wrect.height - 50), scrollPosition, viewRect);
        var options = new Listing_Standard();
        options.Begin(viewRect);
        
        Text.Font = GameFont.Medium;
        options.Label("FCP_Settings_VATS_header".Translate());
        Text.Font = GameFont.Small;
        options.GapLine();
        
        try
        {
            if (options.ButtonText("FCP_VATS_Settings_Reset".Translate()))
            {
                enableSlowDownTime = true;
                enableZoom = true;
                zoomTimeout = 150;
                cooldownTicks = 600;
                flatHitChanceBoost = 0.2f;
                ResetMultipliers();
            }

            options.Gap();

            options.Label("FCP_VATS_Settings_FlatHitChanceBoost".Translate(flatHitChanceBoost.ToString("F5")),
                tooltip: "FCP_VATS_Settings_FlatHitChanceBoost_Mouseover".Translate());
            flatHitChanceBoost = options.Slider(flatHitChanceBoost, 0f, 1f);
            options.Gap();

            options.CheckboxLabeled("FCP_VATS_Settings_EnableSlowDownTime".Translate(), ref enableSlowDownTime);
            options.Gap();

            options.CheckboxLabeled("FCP_VATS_Settings_EnableZoom".Translate(), ref enableZoom);
            options.Gap();

            options.Label("FCP_VATS_Settings_ZoomTimeout".Translate(zoomTimeout));
            options.IntAdjuster(ref zoomTimeout, 1, 60);
            options.Gap();

            options.Label("FCP_VATS_Settings_CooldownTicks".Translate(cooldownTicks));
            options.IntAdjuster(ref cooldownTicks, 1, 60);
            options.Gap();

            options.Label("FCP_VATS_Settings_BodyPartMultipliers".Translate());

            foreach (string defName in multiplierLookup.Keys.ToList())
            {
                BodyPartDef def = DefDatabase<BodyPartDef>.GetNamed(defName);
                if (def == null)
                {
                    continue;
                }

                string label = def.LabelCap;
                options.Label($"[{multiplierLookup[defName]:F5}] {label} - {def.description}");
                multiplierLookup[defName] = options.Slider(multiplierLookup[defName], 0.01f, 10f);
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

        Scribe_Values.Look(ref enableSlowDownTime, "EnableSlowDownTime", true);
        Scribe_Values.Look(ref enableZoom, "EnableZoom", true);
        Scribe_Values.Look(ref zoomTimeout, "ZoomTimeout", 150);
        Scribe_Values.Look(ref cooldownTicks, "CooldownTicks", 600);
        Scribe_Values.Look(ref flatHitChanceBoost, "FlatHitChanceBoost", 0.2f);
        Scribe_Collections.Look(ref multiplierLookup, "MultiplierLookup", LookMode.Value, LookMode.Value);
    }

    public void ResetMultipliers()
    {
        multiplierLookup = new Dictionary<string, float>
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

    private void PopulateMissingMultipliers()
    {
        if (multiplierLookup.Count >= DefDatabase<BodyPartDef>.AllDefs.Count())
        {
            return;
        }

        IEnumerable<BodyPartDef> defs = DefDatabase<BodyPartDef>.AllDefs
            .Where(def => !multiplierLookup.Keys.Contains(def.defName));

        foreach (BodyPartDef bodyPartDef in defs)
        {
            multiplierLookup[bodyPartDef.defName] = 1.0f;
        }
    }
}
