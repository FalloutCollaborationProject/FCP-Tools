using FCP.Enlist;
using UnityEngine;

namespace FCP.Core;

public class EnlistSettings : SettingsTab
{
    public override string TabName => "FCP_Settings_Enlistment".Translate();

    public Dictionary<string, bool> enlistStates = new Dictionary<string, bool>();

    [UsedImplicitly] public Action OnSaved;

    private List<string> enlistKeys;
    private List<bool> boolValues;
    private static Vector2 scrollPosition = Vector2.zero;

    public override void ExposeData()
    {
        Scribe_Collections.Look(ref enlistStates, "enlistStates", LookMode.Value, LookMode.Value, ref enlistKeys, ref boolValues);
        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            enlistStates ??= new Dictionary<string, bool>();
        }
    }

    private static string DisplayLabelFor(string defName)
    {
        return DefDatabase<FactionEnlistOptionsDef>.GetNamed(defName, false)?.label ?? defName;
    }

    public override void DoTabWindowContents(Rect tabRect)
    {
        GUI.color = TerminalColors.PrimaryColor;
        var keys = enlistStates.Keys.ToList().OrderBy(DisplayLabelFor).ToList();
        var viewRect = new Rect(0f, 0f, tabRect.width - 30f, 90 + (keys.Count * 24));
        Widgets.BeginScrollView(tabRect, ref scrollPosition, viewRect);

        var listing = new Listing_Standard();
        listing.Begin(viewRect);
        Text.Font = GameFont.Medium;
        GUI.color = TerminalColors.HighlightColor;
        listing.Label("FCP_Settings_Enlistment_ActiveTypes".Translate());
        GUI.color = TerminalColors.PrimaryColor;
        Text.Font = GameFont.Small;
        listing.Label("FCP_Settings_Enlistment_ActiveTypes_Desc".Translate());
        listing.GapLine();
        foreach (string key in keys)
        {
            var val = enlistStates[key];
            var label = DisplayLabelFor(key);
            listing.CheckboxLabeled(label, ref val, "FCP_Settings_Enlistment_Checkbox_Desc".Translate(label));
            enlistStates[key] = val;
        }
        listing.End();
        Widgets.EndScrollView();
        GUI.color = Color.white;
    }

    public override void OnWriteSettings()
    {
        OnSaved?.Invoke();
    }
}
