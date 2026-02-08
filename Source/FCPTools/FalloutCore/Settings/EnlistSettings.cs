using UnityEngine;

namespace FCP.Core;

public class EnlistSettings : SettingsTab
{
    public override string TabName => "FCP_Settings_Enlistment".Translate();

    public Dictionary<string, bool> enlistStates = new Dictionary<string, bool>();
    
    // To be used from the enlistment assembly until we merge it in.
    // ReSharper disable once InconsistentNaming
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

    public override void DoTabWindowContents(Rect tabRect)
    {
        var keys = enlistStates.Keys.ToList().OrderByDescending(x => x).ToList();
        var viewRect = new Rect(0f, 0f, tabRect.width - 30f, 30 + (keys.Count * 24));
        Widgets.BeginScrollView(tabRect, ref scrollPosition, viewRect);
        
        var listing = new Listing_Standard();
        listing.Begin(viewRect);
        Text.Font = GameFont.Medium;
        listing.Label("FCP_Settings_Enlistment_ActiveTypes".Translate());
        Text.Font = GameFont.Small;
        listing.GapLine();
        for (int num = keys.Count - 1; num >= 0; num--)
        {
            var val = enlistStates[keys[num]];
            listing.CheckboxLabeled(keys[num], ref val);
            enlistStates[keys[num]] = val;
        }
        listing.End();
        Widgets.EndScrollView();
    }

    public override void OnWriteSettings()
    {
        OnSaved?.Invoke();
    }
}
