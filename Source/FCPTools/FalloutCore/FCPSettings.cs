using FCP.Core.VATS;
// ReSharper disable InconsistentNaming

namespace FCP.Core;

public class FCPSettings : ModSettings
{
    public InfoSettings Info = new InfoSettings();
    public GeneralSettings General = new GeneralSettings();
    public VATSSettings VATS = new VATSSettings();
    public EnlistSettings Enlist = new EnlistSettings();
    public TentsSettings Tents = new TentsSettings();
    public ScenarioSettings Scenarios = new ScenarioSettings();
    public DebugSettings Debug = new DebugSettings();

    public IReadOnlyList<SettingsTab> Tabs => [Info, General, VATS, Enlist, Scenarios, Debug];
    
    private Dictionary<Type, SettingsTab> _tabsByType;
    private Dictionary<Type, SettingsTab> TabsByType
        => _tabsByType ??= Tabs.ToDictionary(tab => tab.GetType());

    public T GetTab<T>() where T : SettingsTab
        => TabsByType.TryGetValue(typeof(T), out SettingsTab tab) ? (T)tab : null;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Deep.Look(ref Info, nameof(Info));
        Scribe_Deep.Look(ref General, nameof(General));
        Scribe_Deep.Look(ref VATS, nameof(VATS));
        Scribe_Deep.Look(ref Enlist, nameof(Enlist));
        Scribe_Deep.Look(ref Tents, nameof(Tents));
        Scribe_Deep.Look(ref Scenarios, nameof(Scenarios));
        Scribe_Deep.Look(ref Debug, nameof(Debug));

        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            Info ??= new InfoSettings();
            General ??= new GeneralSettings();
            VATS ??= new VATSSettings();
            Enlist ??= new EnlistSettings();
            Tents ??= new TentsSettings();
            Scenarios ??= new ScenarioSettings();
            Debug ??= new DebugSettings();
        }
    }
}