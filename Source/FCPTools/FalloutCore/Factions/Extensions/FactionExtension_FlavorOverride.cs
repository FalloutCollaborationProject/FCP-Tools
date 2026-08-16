namespace FCP.Factions;

[UsedImplicitly]
public class FactionExtension_FlavorOverride : DefModExtension
{
    public string newLabel;
    public bool preferFactionLeaderTitle = false;

    [CanBeNull]
    public static string TryGetLabel(FactionDef factionDef)
    {
        var ext = factionDef.GetModExtension<FactionExtension_FlavorOverride>();
        return ext?.newLabel;
    }
}