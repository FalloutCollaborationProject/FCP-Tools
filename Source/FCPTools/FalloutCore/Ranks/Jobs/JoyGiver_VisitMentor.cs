using Verse.AI;

namespace FCP.Ranks.Jobs;

public class JoyGiver_VisitMentor : JoyGiver
{
    private const float JuvenileChance = 9f;

    public override bool CanBeGivenTo(Pawn pawn)
    {
        if (!base.CanBeGivenTo(pawn))
            return false;

        PawnRankData data = Current.Game.GetComponent<GameComponent_PawnRanks>()?.GetRankData(pawn);
        return data?.mentor != null && RankTrackUtility.IsFactionActive(data.track);
    }

    public override float GetChance(Pawn pawn)
    {
        return pawn.DevelopmentalStage.Juvenile() ? JuvenileChance : def.baseChance;
    }

    public override Job TryGiveJob(Pawn pawn)
    {
        Pawn mentor = Current.Game.GetComponent<GameComponent_PawnRanks>()?.GetRankData(pawn)?.mentor;
        if (mentor == null || !mentor.Spawned || mentor.Map != pawn.Map || mentor.Downed || mentor.Dead)
            return null;

        if (!pawn.CanReach(mentor, PathEndMode.Touch, Danger.Some))
            return null;

        if (!pawn.CanReserve(mentor, 1, -1, null, false))
            return null;

        return JobMaker.MakeJob(def.jobDef, mentor);
    }
}
