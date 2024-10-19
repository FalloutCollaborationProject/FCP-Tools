namespace FCP.Core;

[DefOf]
public class FCPDefOf
{
    [MayRequire("Rick.FCP.Legion")]
    public static FactionDef FCP_Faction_Caesars_Legion;
        
    [MayRequire("Rick.FCP.NCR")]
    public static FactionDef FCP_Faction_NCR;
    
    public static PawnGroupKindDef FCP_PawnGroupKind_TaxCollector;
    public static LetterDef FCP_Letter_AcceptStoryteller;
    public static JobDef FCP_AICastAbilityAtTarget;
    
    static FCPDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(FCPDefOf));
    }
}