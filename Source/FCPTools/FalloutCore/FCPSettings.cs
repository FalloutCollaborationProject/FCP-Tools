using FCP.Core.VATS;
// ReSharper disable InconsistentNaming

namespace FCP.Core;

public class FCPSettings : ModSettings
{
    public GeneralSettings General = new GeneralSettings();
    public VATSSettings VATS = new VATSSettings();
    public EnlistSettings Enlist = new EnlistSettings();
    public DebugSettings Debug = new DebugSettings();

    public IReadOnlyList<SettingsTab> Tabs => [General, VATS, Enlist, Debug];

    private Dictionary<Type, SettingsTab> TabsByType
        => field ??= Tabs.ToDictionary(tab => tab.GetType());

    public T GetTab<T>() where T : SettingsTab
        => TabsByType.TryGetValue(typeof(T), out SettingsTab tab) ? (T)tab : null;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Deep.Look(ref General, nameof(General));
        Scribe_Deep.Look(ref VATS, nameof(VATS));
        Scribe_Deep.Look(ref Enlist, nameof(Enlist));
        Scribe_Deep.Look(ref Debug, nameof(Debug));

        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            General ??= new GeneralSettings();
            VATS ??= new VATSSettings();
            Enlist ??= new EnlistSettings();
            Debug ??= new DebugSettings();
        }
    }
}