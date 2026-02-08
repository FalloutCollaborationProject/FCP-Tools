using UnityEngine;

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
        int restHeight = 248 + 64;
        float scrollViewHeight = multiplierHeight + restHeight; // Adjust this value as needed
        Rect viewRect = new Rect(0, 0, wrect.width - 20, scrollViewHeight);
        scrollPosition = GUI.BeginScrollView(new Rect(0, 50, wrect.width, wrect.height - 50), scrollPosition, viewRect);
        Listing_Standard options = new Listing_Standard();
        options.Begin(viewRect);
        try
        {
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
