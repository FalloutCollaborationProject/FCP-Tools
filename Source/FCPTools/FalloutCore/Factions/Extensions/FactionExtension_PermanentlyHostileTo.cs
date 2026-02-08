namespace FCP.Factions;

public class FactionExtension_PermanentlyHostileTo : DefModExtension
{
    [UsedImplicitly] public List<FactionDef> hostileFactionDefs;

    public bool FactionIsHostileTo(FactionDef other) => hostileFactionDefs.Contains(other);
}