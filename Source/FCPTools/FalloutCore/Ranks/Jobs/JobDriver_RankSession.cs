using Verse.AI;

namespace FCP.Ranks.Jobs;

public abstract class JobDriver_RankSession : JobDriver
{
    private const int PartnerGapCells = 2;

    protected virtual int SessionDuration => 5000;

    protected virtual JobDef PartnerJobDef => null;

    protected Pawn Partner => (Pawn)job.GetTarget(TargetIndex.A).Thing;

    protected static GameComponent_PawnRanks RankComp => Current.Game.GetComponent<GameComponent_PawnRanks>();

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return pawn.Reserve(TargetA, job, 1, -1, null, errorOnFailed);
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
        this.FailOnDowned(TargetIndex.A);
        this.FailOnMentalState(TargetIndex.A);

        AddFinishAction(EndPartnerSession);

        Toil findSpot = ToilMaker.MakeToil("FindSessionSpot");
        findSpot.initAction = delegate
        {
            job.SetTarget(TargetIndex.B, FindSessionSpot());
        };
        findSpot.defaultCompleteMode = ToilCompleteMode.Instant;
        yield return findSpot;

        yield return Toils_Goto.GotoCell(TargetIndex.B, PathEndMode.OnCell);

        yield return Toils_General.Do(StartPartnerSession);

        Toil session = Toils_General.Wait(SessionDuration).WithProgressBarToilDelay(TargetIndex.A);
        session.tickAction = delegate
        {
            pawn.rotationTracker.FaceTarget(Partner);
            TickAction();
        };
        yield return session;

        yield return Toils_General.Do(Complete);
    }

    private IntVec3 FindSessionSpot()
    {
        Pawn partner = Partner;
        if (partner?.Map == null)
            return pawn.Position;

        IntVec3 spot = CellFinder.RandomClosewalkCellNear(partner.Position, partner.Map, PartnerGapCells + 2,
            c => (c - partner.Position).LengthHorizontalSquared >= PartnerGapCells * PartnerGapCells && c.Standable(partner.Map));

        return spot.IsValid ? spot : partner.Position;
    }

    private void StartPartnerSession()
    {
        if (Partner?.jobs == null || PartnerJobDef == null)
            return;

        Job partnerJob = JobMaker.MakeJob(PartnerJobDef, pawn);
        partnerJob.expiryInterval = SessionDuration;
        Partner.jobs.StartJob(partnerJob, JobCondition.InterruptForced, resumeCurJobAfterwards: true);
    }

    private void EndPartnerSession(JobCondition condition)
    {
        if (Partner?.jobs?.curDriver is JobDriver_RankSessionPartner)
            Partner.jobs.EndCurrentJob(JobCondition.InterruptForced, false);
    }

    protected abstract void TickAction();
    protected abstract void Complete();

    protected static void GrantXp(Pawn learner, List<SkillGain> gains, float rateMultiplier = 1f)
    {
        if (gains == null || learner.skills == null)
            return;

        foreach (SkillGain gain in gains)
            learner.skills.Learn(gain.skill, gain.amount / (float)GenDate.TicksPerHour * rateMultiplier);
    }
}
