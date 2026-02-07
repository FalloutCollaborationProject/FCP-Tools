using FCP.Core.Settings;
using DebugSettings = FCP.Core.Settings.DebugSettings;

namespace FCP.Core;

public class FCPSettings : ModSettings
{
    private List<SettingsTab> tabs;
    public IReadOnlyList<SettingsTab> Tabs => tabs;

    public T GetTab<T>() where T : SettingsTab
        => tabs.OfType<T>().FirstOrDefault();

    public bool HasTab<T>() where T : SettingsTab
        => tabs.OfType<T>().Any();

    public override void ExposeData()
    {
        tabs ??= [];

        AddTabTypeIfNone<GeneralSettings>();
        AddTabTypeIfNone<DebugSettings>();
        
        base.ExposeData();
        Scribe_Collections.Look(ref tabs, nameof(tabs), LookMode.Deep);
    }

    private void AddTabTypeIfNone<T>() where T : SettingsTab, new()
    {
        if (!tabs.ContainsAny(tab => tab is T))
            tabs.Add(new T());
    }
}