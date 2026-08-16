using Verse.AI;

namespace FCP.Ranks.Jobs;

public class JobDriver_PunishmentDetail : JobDriver
{
    private const int Duration = 2500;
    private const int LaborRadius = 6;
    private const int DustPuffIntervalTicks = 90;

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return true;
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        Toil findSpot = ToilMaker.MakeToil("FindPunishmentSpot");
        findSpot.initAction = delegate
        {
            IntVec3 spot = CellFinder.RandomClosewalkCellNear(pawn.Position, pawn.Map, LaborRadius);
            job.SetTarget(TargetIndex.A, spot.IsValid ? spot : pawn.Position);
        };
        findSpot.defaultCompleteMode = ToilCompleteMode.Instant;
        yield return findSpot;

        yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);

        Toil labor = Toils_General.Wait(Duration).WithProgressBarToilDelay(TargetIndex.A);
        labor.handlingFacing = true;
        labor.tickAction = delegate
        {
            if (Find.TickManager.TicksGame % DustPuffIntervalTicks == 0)
                FleckMaker.ThrowDustPuff(pawn.Position, pawn.Map, 1f);
        };
        yield return labor;

        yield return Toils_General.Do(Complete);
    }

    private void Complete()
    {
        pawn.needs?.mood?.thoughts.memories.TryGainMemory(RankThoughtDefOf.FCP_Thought_PunishmentDetail);
        Messages.Message("FCP_Rank_PunishmentComplete".Translate(pawn.LabelShortCap), pawn, MessageTypeDefOf.NeutralEvent);
    }
}
