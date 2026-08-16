using FCP.Ranks.Jobs;
using UnityEngine;
using Verse.AI;

namespace FCP.Ranks;

public class GameComponent_PawnRanks : GameComponent
{
    private Dictionary<Pawn, PawnRankData> ranks = new Dictionary<Pawn, PawnRankData>();

    public GameComponent_PawnRanks(Game game)
    {
    }

    public IEnumerable<KeyValuePair<Pawn, PawnRankData>> AllTracked => ranks;

    public bool IsTracked(Pawn pawn)
    {
        return pawn != null && ranks.ContainsKey(pawn);
    }

    public PawnRankData GetRankData(Pawn pawn)
    {
        if (pawn == null)
            return null;

        ranks.TryGetValue(pawn, out PawnRankData result);
        return result;
    }

    public void Enroll(Pawn pawn, RankTrackDef track, bool devForce = false)
    {
        if (pawn == null || track?.rootTier == null)
            return;

        if (!devForce && !RankTrackUtility.IsFactionActive(track))
            return;

        PawnRankData existing = GetRankData(pawn);
        if (existing != null && (existing.track == track || (!devForce && RankTrackUtility.IsFactionActive(existing.track))))
            return;

        PawnRankData data = new PawnRankData { track = track };
        ranks[pawn] = data;
        RankTrackUtility.Promote(pawn, data, track.rootTier);
    }

    public void SetMentor(Pawn pawn, Pawn mentor)
    {
        PawnRankData data = GetRankData(pawn);
        if (data != null)
            data.mentor = mentor;
    }

    public List<RankTierDef> GetPromotionOptions(Pawn pawn)
    {
        return GetRankData(pawn)?.currentTier?.nextTierOptions;
    }

    public void Promote(Pawn pawn, RankTierDef chosenTier)
    {
        PawnRankData data = GetRankData(pawn);
        if (data != null && chosenTier != null)
            RankTrackUtility.Promote(pawn, data, chosenTier);
    }

    private const int AuraTickInterval = 250;

    public override void GameComponentTick()
    {
        if (Find.TickManager.TicksGame % AuraTickInterval == 0)
            ApplyLeadershipAuras();

        if (Find.TickManager.TicksGame % 60000 != 0)
            return;

        foreach (KeyValuePair<Pawn, PawnRankData> entry in ranks)
        {
            if (entry.Key == null || entry.Key.Dead)
                continue;

            List<RankTierDef> options = entry.Value.currentTier?.nextTierOptions;
            if (options.NullOrEmpty() || options.Count > 1)
                continue;

            RankTierDef candidate = options[0];
            if (RankTrackUtility.MeetsRequirements(entry.Key, entry.Value, candidate))
                RankTrackUtility.Promote(entry.Key, entry.Value, candidate);
        }
    }

    private void ApplyLeadershipAuras()
    {
        foreach (KeyValuePair<Pawn, PawnRankData> entry in ranks)
        {
            Pawn commander = entry.Key;
            RankTierDef tier = entry.Value.currentTier;
            if (tier == null || !tier.isCommanderTier || tier.auraHediff == null)
                continue;

            if (commander == null || commander.Dead || !commander.Spawned || commander.Map == null)
                continue;

            if (!RankTrackUtility.IsFactionActive(entry.Value.track))
                continue;

            foreach (Pawn other in commander.Map.mapPawns.FreeColonists)
            {
                if (other == commander || other.health == null)
                    continue;

                PawnRankData otherData = GetRankData(other);
                if (otherData == null || otherData.track != entry.Value.track || !RankTrackUtility.IsMilitaryTier(otherData.currentTier))
                    continue;

                if (!other.Position.InHorDistOf(commander.Position, tier.auraRadius))
                    continue;

                Hediff existing = other.health.hediffSet.hediffs.FirstOrDefault(h => h.def == tier.auraHediff);
                if (existing == null)
                {
                    other.health.AddHediff(tier.auraHediff);
                }
                else
                {
                    HediffComp_Disappears disappears = existing.TryGetComp<HediffComp_Disappears>();
                    if (disappears != null)
                        disappears.ticksToDisappear = AuraTickInterval + 60;
                }
            }
        }
    }

    public void Rally(Pawn commander)
    {
        PawnRankData data = GetRankData(commander);
        if (data?.currentTier == null || !data.currentTier.isCommanderTier || data.currentTier.rallyHediff == null)
            return;

        if (commander.Map == null)
            return;

        foreach (Pawn other in commander.Map.mapPawns.FreeColonists)
        {
            PawnRankData otherData = GetRankData(other);
            if (otherData == null || otherData.track != data.track || other.health == null || !RankTrackUtility.IsMilitaryTier(otherData.currentTier))
                continue;

            if (!other.health.hediffSet.HasHediff(data.currentTier.rallyHediff))
                other.health.AddHediff(data.currentTier.rallyHediff);
        }

        data.lastRallyTick = Find.TickManager.TicksGame;
        data.AddHistory("FCP_Rank_History_Rallied".Translate(data.track.collectiveNoun));

        Find.LetterStack.ReceiveLetter(
            "FCP_Rank_Rally_Title".Translate(commander.LabelShortCap, data.track.collectiveNoun),
            "FCP_Rank_Rally_Text".Translate(commander.LabelShortCap, data.track.collectiveNoun),
            LetterDefOf.PositiveEvent,
            commander);
    }

    private const int MaxFormationRows = 4;

    public void FormUp(Pawn commander)
    {
        PawnRankData data = GetRankData(commander);
        if (data?.currentTier == null || !data.currentTier.isCommanderTier)
            return;

        if (commander.Map == null)
            return;

        List<Pawn> subordinates = new List<Pawn>();
        foreach (Pawn other in commander.Map.mapPawns.FreeColonists)
        {
            if (other == commander || other.Downed || other.Dead || other.InMentalState || other.jobs == null)
                continue;

            PawnRankData otherData = GetRankData(other);
            if (otherData == null || otherData.track != data.track || !RankTrackUtility.IsMilitaryTier(otherData.currentTier))
                continue;

            subordinates.Add(other);
        }

        if (subordinates.Count == 0)
            return;

        int cols = Mathf.Max(1, Mathf.CeilToInt(subordinates.Count / (float)MaxFormationRows));
        IntVec3 behind = commander.Rotation.FacingCell * -1;
        IntVec3 right = commander.Rotation.Rotated(RotationDirection.Clockwise).FacingCell;

        for (int i = 0; i < subordinates.Count; i++)
        {
            int row = i / cols + 1;
            int colOffset = (i % cols) - (cols - 1) / 2;

            IntVec3 cell = commander.Position + behind * row + right * colOffset;
            if (!cell.InBounds(commander.Map) || !cell.Standable(commander.Map))
            {
                IntVec3 fallback = CellFinder.RandomClosewalkCellNear(cell, commander.Map, 2);
                if (fallback.IsValid)
                    cell = fallback;
            }

            Job formJob = JobMaker.MakeJob(RankJobDefOf.FCP_Job_FormUp, commander);
            formJob.SetTarget(TargetIndex.B, cell);
            subordinates[i].jobs.StartJob(formJob, JobCondition.InterruptForced, resumeCurJobAfterwards: true);
        }

        data.lastFormUpTick = Find.TickManager.TicksGame;
        data.AddHistory("FCP_Rank_History_FormedUp".Translate(data.track.collectiveNoun));

        Messages.Message("FCP_Rank_FormUp_Message".Translate(commander.LabelShortCap, data.track.collectiveNoun), commander, MessageTypeDefOf.NeutralEvent);
    }

    public bool HasActiveFormation(Pawn commander)
    {
        if (commander?.Map == null)
            return false;

        foreach (Pawn other in commander.Map.mapPawns.FreeColonists)
        {
            if (IsFormedUpFor(other, commander))
                return true;
        }

        return false;
    }

    public void Dismiss(Pawn commander)
    {
        if (commander?.Map == null)
            return;

        int dismissed = 0;
        foreach (Pawn other in commander.Map.mapPawns.FreeColonists.ToList())
        {
            if (!IsFormedUpFor(other, commander))
                continue;

            other.jobs.EndCurrentJob(JobCondition.InterruptForced);
            dismissed++;
        }

        if (dismissed > 0)
        {
            PawnRankData data = GetRankData(commander);
            string collectiveNoun = data?.track?.collectiveNoun ?? "the order";
            Messages.Message("FCP_Rank_Dismiss_Message".Translate(commander.LabelShortCap, collectiveNoun), commander, MessageTypeDefOf.NeutralEvent);
        }
    }

    private static bool IsFormedUpFor(Pawn pawn, Pawn commander)
    {
        Job job = pawn.jobs?.curJob;
        return job != null && job.def == RankJobDefOf.FCP_Job_FormUp && job.GetTarget(TargetIndex.A).Thing == commander;
    }

    private List<Pawn> rankKeys;
    private List<PawnRankData> rankValues;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Collections.Look(ref ranks, "ranks", LookMode.Reference, LookMode.Deep, ref rankKeys, ref rankValues);

        if (Scribe.mode == LoadSaveMode.LoadingVars && ranks == null)
            ranks = new Dictionary<Pawn, PawnRankData>();
    }
}
