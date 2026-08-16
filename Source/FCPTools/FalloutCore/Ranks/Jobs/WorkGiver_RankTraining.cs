using Verse.AI;

namespace FCP.Ranks.Jobs;

public class WorkGiver_RankTraining : WorkGiver_Scanner
{
    private const int CooldownTicks = GenDate.TicksPerDay;

    public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.Pawn);

    public override PathEndMode PathEndMode => PathEndMode.ClosestTouch;

    public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
    {
        if (t is not Pawn mentor || !mentor.RaceProps.Humanlike)
            return false;

        GameComponent_PawnRanks comp = Current.Game.GetComponent<GameComponent_PawnRanks>();
        PawnRankData data = comp?.GetRankData(pawn);
        if (data == null || data.mentor != mentor || data.currentTier.nextTierOptions.NullOrEmpty())
            return false;

        if (!RankTrackUtility.IsFactionActive(data.track))
            return false;

        if (data.lastTrainedTick >= 0 && Find.TickManager.TicksGame - data.lastTrainedTick < CooldownTicks)
            return false;

        if (mentor.Downed || mentor.Dead || mentor.Drafted || mentor.InMentalState)
            return false;

        return pawn.CanReserve(mentor, 1, -1, null, forced);
    }

    public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
    {
        return JobMaker.MakeJob(RankJobDefOf.FCP_Job_RankTraining, t);
    }
}
