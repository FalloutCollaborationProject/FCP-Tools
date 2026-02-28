namespace FCP.Factions;

/// <summary>
/// Works to override the faction's label (so the NCR's label isn't always New California Republic, same as its name)
/// Also supports prioritizing faction leader title
/// </summary>
[UsedImplicitly]
public class FactionExtension_FlavorOverride : DefModExtension
{
    public string newLabel;
    public bool preferFactionLeaderTitle = false;

    public static string TryGetLabel(FactionDef factionDef)
    {
        var ext = factionDef.GetModExtension<FactionExtension_FlavorOverride>();
        return ext?.newLabel;
    }
}