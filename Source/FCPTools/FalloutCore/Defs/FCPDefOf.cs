using RimWorld;
using Verse;
using Verse.AI;

// Changed back to block-scoped namespace
namespace FCP.Core
{
    [DefOf]
    public static class FCPDefOf // Made static as is convention for DefOf classes
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
        public static DutyDef FCP_HuntInRadius;
        public static DutyDef FCP_ForageInRadius;
        public static DutyDef FCP_CookAtCampfire;
        public static RecipeDef CookMeal_Simple;
        public static RecipeDef ButcherCorpseFlesh;
        static FCPDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(FCPDefOf));
        }
    }
}