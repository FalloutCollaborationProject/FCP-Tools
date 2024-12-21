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
    
    public static JobDef FCP_VATS_AttackHybrid;
    public static ThingDef FCP_VATS_Zoomer;
    public static StatCategoryDef FCP_LegendaryEffectStats;
    public static EffecterDef FCP_VATSLegendaryEffect_Explosive_Explosion;

    public static LegendaryEffectDef FCP_VATSLegendaryEffect_Rapid;
    
    static FCPDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(FCPDefOf));
    }
}