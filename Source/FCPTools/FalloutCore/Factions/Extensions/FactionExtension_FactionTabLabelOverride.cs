namespace FCP.Factions;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class FactionExtension_FactionTabLabelOverride : DefModExtension
{
    public string newLabel;

    public static string TryGetLabel(FactionDef factionDef)
    {
        var ext = factionDef.GetModExtension<FactionExtension_FactionTabLabelOverride>();
        return ext?.newLabel;
    }
}