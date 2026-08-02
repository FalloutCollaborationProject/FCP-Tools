namespace FCP.Factions;

public class FactionExtension_PermanentlyAlliedTo : DefModExtension
{
    [UsedImplicitly] public List<FactionDef> alliedFactionDefs;

    public bool FactionIsAlliedTo(FactionDef other) => alliedFactionDefs.Contains(other);
}
