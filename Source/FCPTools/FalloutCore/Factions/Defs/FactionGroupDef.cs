namespace FCP.Factions;

public class FactionGroupDef : Def
{
    public FactionDef leadingFaction;
    public List<FactionDef> factions;
    public List<FactionDef> playerFactions;
}