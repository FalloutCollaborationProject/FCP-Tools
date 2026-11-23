namespace FCP.Railroad;

[DefOf]
public static class RailroadDefOf
{
    [MayRequire("Rick.FCP.Railroad")]
    public static FactionDef FCP_Faction_Railroad;
    [MayRequire("Rick.FCP.Railroad")]
    public static LetterDef FCP_Letter_RailroadLiberation;
    [MayRequire("Rick.FCP.Railroad")]
    public static ThoughtDef FCP_Thought_RailroadLiberated;

    [MayRequire("Rick.FCP.Institute")]
    public static HediffDef FCP_Hediff_SynthComponents_GenIII;

    static RailroadDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(RailroadDefOf));
    }
}
