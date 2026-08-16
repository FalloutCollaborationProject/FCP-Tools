namespace FCP.Ranks;

[DefOf]
public static class RankHediffDefOf
{
    public static HediffDef FCP_Hediff_MentorDown_Duty;
    public static HediffDef FCP_Hediff_MentorWellMaintained;
    public static HediffDef FCP_Hediff_Inspection_Commended;
    public static HediffDef FCP_Hediff_Inspection_Reprimanded;

    static RankHediffDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(RankHediffDefOf));
    }
}
