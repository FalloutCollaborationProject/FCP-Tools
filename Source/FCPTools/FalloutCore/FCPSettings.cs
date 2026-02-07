using FCP.Core.VATS;
// ReSharper disable InconsistentNaming

namespace FCP.Core;

public class FCPSettings : ModSettings
{
    public GeneralSettings General = new GeneralSettings();
    public DebugSettings Debug = new DebugSettings();
    public VATSSettings VATS = new VATSSettings();

    public IReadOnlyList<SettingsTab> Tabs => [General, Debug, VATS];

    public T GetTab<T>() where T : SettingsTab
        => Tabs.OfType<T>().FirstOrDefault();

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Deep.Look(ref General, nameof(General));
        Scribe_Deep.Look(ref Debug, nameof(Debug));
        Scribe_Deep.Look(ref VATS, nameof(VATS));

        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            General ??= new GeneralSettings();
            Debug ??= new DebugSettings();
            VATS ??= new VATSSettings();
        }
    }
}