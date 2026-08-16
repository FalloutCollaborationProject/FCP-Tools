namespace FCP.Ranks.Jobs;

public class JobDriver_RankTraining : JobDriver_RankSession
{
    protected override void TickAction()
    {
        PawnRankData data = RankComp?.GetRankData(pawn);
        GrantXp(pawn, data?.currentTier?.trainingXpPerHour);

        if (pawn.DevelopmentalStage.Juvenile() && pawn.needs?.learning != null)
            pawn.needs.learning.CurLevel += 0.25f / GenDate.TicksPerHour;

        Partner.skills?.Learn(SkillDefOf.Social, 0.1f / GenDate.TicksPerHour);
    }

    protected override void Complete()
    {
        PawnRankData data = RankComp?.GetRankData(pawn);
        if (data != null)
            data.lastTrainedTick = Find.TickManager.TicksGame;

        Messages.Message("FCP_Rank_TrainingComplete".Translate(pawn.LabelShortCap, Partner.LabelShortCap), pawn, MessageTypeDefOf.PositiveEvent);
        pawn.needs?.mood?.thoughts.memories.TryGainMemory(RankThoughtDefOf.FCP_Thought_RankTrained);
    }
}
