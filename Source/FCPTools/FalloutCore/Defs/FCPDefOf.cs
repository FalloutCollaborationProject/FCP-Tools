namespace FCP.Core;

[DefOf]
public class FCPDefOf
{
    [MayRequire("Rick.FCP.Legion")]
    public static FactionDef FCP_Faction_Caesars_Legion;

    [MayRequire("Rick.FCP.NCR")]
    public static FactionDef FCP_Faction_NCR;

    [MayRequire("Rick.FCP.COA")]
    public static TraitDef FCP_Trait_ZealotOfAtom;

    [MayRequire("Rick.FCP.COA")]
    public static TraitDef FCP_Trait_CongregantOfAtom;

    [MayRequire("Rick.FCP.Institute")]
    public static TraitDef FCP_Trait_Institute_Loyalist;

    [MayRequire("Rick.FCP.Railroad")]
    public static TraitDef FCP_Trait_Railroad_Sympathizer;

    [MayRequire("Rick.FCP.BOS")]
    public static TraitDef FCP_Trait_BoS_Zealot;

    public static PawnGroupKindDef FCP_PawnGroupKind_TaxCollector;
    public static LetterDef FCP_Letter_AcceptStoryteller;
    public static LetterDef FCP_Letter_Squatters;

    [MayRequire("Rick.FCP.Robotics")]
    public static LetterDef FCP_Letter_AcceptRobotJoin;
    public static JobDef FCP_AICastAbilityAtTarget;

    public static JobDef FCP_VATS_AttackHybrid;
    public static ThingDef FCP_VATS_Zoomer;
    public static EffecterDef FCP_VATSLegendaryEffect_Explosive_Explosion;

    public static KeyBindingDef FCP_VatsKeyBinding;

    public static HediffDef FCP_VATSCrippledHediff;
    public static HediffDef FCP_VATSPoisoning;

    [MayRequire("Rick.FCP.Animals")]
    public static ThingDef FCP_PawnFlyer_BurrowAmbush;
    static FCPDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(FCPDefOf));
    }
}