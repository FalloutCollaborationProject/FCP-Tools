namespace FCP.Core;

public class ModExtension_FactionPermanentlyHostileTo : DefModExtension
{
    [UsedImplicitly] 
    public List<FactionDef> hostileFactionDefs;

    public bool FactionIsHostileTo(FactionDef other) => hostileFactionDefs.Contains(other);
}