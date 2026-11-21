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
    public static ShaderTypeDef FCP_VATSZoom;
    public static EffecterDef FCP_VATSLegendaryEffect_Explosive_Explosion;

    public static KeyBindingDef FCP_VatsKeyBinding;

    public static HediffDef FCP_VATSCrippledHediff;
    public static HediffDef FCP_VATSPoisoning;

    [MayRequire("Rick.FCP.Animals")]
    public static ThingDef FCP_PawnFlyer_BurrowAmbush;

    [MayRequire("Rick.FCP.NCR")]
    public static ThingDef FCP_Building_NCR_Tent;

    public static MentalStateDef FCP_MentalState_PermanentBerserk;
    public static ResearchTabDef FCP_Pre_War_Research;

    [MayRequire("Rick.FCP.Lore")]
    public static ThingDef FCP_Production_Old_Wall_Terminal;
    [MayRequire("Rick.FCP.Lore")]
    public static ThingDef FCP_Production_Clean_Wall_Terminal;

    [MayRequire("Rick.FCP.Ghouls")]
    public static GeneDef FCP_Gene_Ferality;
    [MayRequire("Rick.FCP.Ghouls")]
    public static GeneDef FCP_Gene_Ghoul_Feral_Skin;
    [MayRequire("Rick.FCP.Ghouls")]
    public static GeneDef FCP_Gene_Ghoul_Skin_GlowingOne;
    [MayRequire("Rick.FCP.Ghouls")]
    public static HediffDef FCP_Hediff_GhoulTransformation;
    [MayRequire("Rick.FCP.Ghouls")]
    public static PawnKindDef FCP_Pawnkind_Ghoul_Feral;
    [MayRequire("Rick.FCP.Ghouls")]
    public static XenotypeDef FCP_Xenotype_Ghoul;
    [MayRequire("Rick.FCP.Ghouls")]
    public static XenotypeDef FCP_Xenotype_Ghoul_Feral;
    [MayRequire("Rick.FCP.Ghouls")]
    public static XenotypeDef FCP_Xenotype_Ghoul_Glowing_One;

    public static WorkGiverDef HaulGeneral;
    public static WorkGiverDef CleanFilth;
    public static WorkGiverDef GrowerSow;
    public static WorkGiverDef GrowerHarvest;
    public static WorkGiverDef DoctorFeedHumanlikes;
    public static WorkGiverDef DoctorTendToHumanlikes;
    public static WorkGiverDef ConstructDeliverResourcesToBlueprints;
    public static WorkGiverDef ConstructDeliverResourcesToFrames;
    public static WorkGiverDef ConstructFinishFrames;
    [MayRequireBiotech]
    public static WorkGiverDef BottleFeedBaby;
    [MayRequireBiotech]
    public static WorkGiverDef PlayWithBaby;

    static FCPDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(FCPDefOf));
    }
}