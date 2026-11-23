namespace FCP.Synths;

[DefOf]
public static class SynthDefOf
{
    [MayRequire("Rick.FCP.Institute")]
    public static HediffDef FCP_Hediff_SynthComponents_GenIII;
    [MayRequire("Rick.FCP.Institute")]
    public static XenotypeDef FCP_Xenotype_Synth_GenI;
    [MayRequire("Rick.FCP.Institute")]
    public static XenotypeDef FCP_Xenotype_Synth_GenII;
    [MayRequire("Rick.FCP.Institute")]
    public static FactionDef FCP_Faction_Institute;
    [MayRequire("Rick.FCP.Institute")]
    public static LetterDef FCP_Letter_InstituteRelocationOffer;
    [MayRequire("Rick.FCP.Institute")]
    public static PawnKindDef FCP_Pawnkind_Institute_RequisitionOfficer;
    [MayRequire("Rick.FCP.Institute")]
    public static TraderKindDef FCP_TraderKind_Institute_Requisition;
    [MayRequire("Rick.FCP.Institute")]
    public static FCP.Enlist.FactionEnlistOptionsDef FCP_Enlistment_Institute;

    static SynthDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(SynthDefOf));
    }
}
