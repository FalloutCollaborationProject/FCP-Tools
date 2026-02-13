namespace FCP.Factions;

/// <summary>
/// Group up factions for relationship / etc purposes (eg. Greater BoS)
/// </summary>
public class FactionGroupDef : Def
{
    public FactionDef leadingFaction;
    public List<FactionDef> factions;
    public List<FactionDef> playerFactions;
}