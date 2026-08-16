namespace FCP.Ranks;

[DefOf]
public static class RankThoughtDefOf
{
    public static ThoughtDef FCP_Thought_RankTrained;
    public static ThoughtDef FCP_Thought_RankDrilled;
    public static ThoughtDef FCP_Thought_RankStudied;
    public static ThoughtDef FCP_Thought_ObservedMentor;
    public static ThoughtDef FCP_Thought_MentorDied;
    public static ThoughtDef FCP_Thought_Inspection_Commended;
    public static ThoughtDef FCP_Thought_Inspection_Reprimanded;
    public static ThoughtDef FCP_Thought_PunishmentDetail;
    public static ThoughtDef FCP_Thought_PromotionCeremony;
    public static ThoughtDef FCP_Thought_WitnessedPromotion;

    static RankThoughtDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(RankThoughtDefOf));
    }
}
