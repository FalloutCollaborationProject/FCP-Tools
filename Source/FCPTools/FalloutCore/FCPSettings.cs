using FCP.Core.VATS;
// ReSharper disable InconsistentNaming

namespace FCP.Core;

public class FCPSettings : ModSettings
{
    public GeneralSettings General = new GeneralSettings();
    public VATSSettings VATS = new VATSSettings();
    public DebugSettings Debug = new DebugSettings();

    public IReadOnlyList<SettingsTab> Tabs => [General, VATS, Debug];

    public T GetTab<T>() where T : SettingsTab
        => Tabs.OfType<T>().FirstOrDefault();

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Deep.Look(ref General, nameof(General));
        Scribe_Deep.Look(ref VATS, nameof(VATS));
        Scribe_Deep.Look(ref Debug, nameof(Debug));

        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            General ??= new GeneralSettings();
            VATS ??= new VATSSettings();
            Debug ??= new DebugSettings();
        }
    }
}