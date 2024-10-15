namespace FCP.Core;

/// <summary>
/// Allows Hidden factions to show up in Caravan Meetings & Arrivals, as long as they are otherwise valid.
/// </summary>
public class ModExtension_HiddenFactionHasCaravans : DefModExtension
{
    public static bool FactionHas(Faction faction)
    {
        return faction.def.HasModExtension<ModExtension_HiddenFactionHasCaravans>();
    }
}
