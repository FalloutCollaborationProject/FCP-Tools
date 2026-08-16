namespace FCP.Ranks;

public class PawnRankData : IExposable
{
    public RankTrackDef track;
    public RankTierDef currentTier;
    public int rankStartTick;
    public Pawn mentor;
    public int squiresTrained;
    public int missionsCompleted;
    public int lastTrainedTick = -1;
    public int lastDrilledTick = -1;
    public int lastStudiedTick = -1;
    public int lastRallyTick = -1;
    public int lastInspectedTick = -1;
    public int lastFormUpTick = -1;
    public int lastPunishedTick = -1;
    public List<RankHistoryEntry> history = new List<RankHistoryEntry>();

    private const int MaxHistoryEntries = 50;

    public void AddHistory(string text)
    {
        history.Add(new RankHistoryEntry { tick = Find.TickManager.TicksGame, text = text });
        if (history.Count > MaxHistoryEntries)
            history.RemoveAt(0);
    }

    public void ExposeData()
    {
        Scribe_Defs.Look(ref track, "track");
        Scribe_Defs.Look(ref currentTier, "currentTier");
        Scribe_Values.Look(ref rankStartTick, "rankStartTick");
        Scribe_References.Look(ref mentor, "mentor");
        Scribe_Values.Look(ref squiresTrained, "squiresTrained");
        Scribe_Values.Look(ref missionsCompleted, "missionsCompleted");
        Scribe_Values.Look(ref lastTrainedTick, "lastTrainedTick", -1);
        Scribe_Values.Look(ref lastDrilledTick, "lastDrilledTick", -1);
        Scribe_Values.Look(ref lastStudiedTick, "lastStudiedTick", -1);
        Scribe_Values.Look(ref lastRallyTick, "lastRallyTick", -1);
        Scribe_Values.Look(ref lastInspectedTick, "lastInspectedTick", -1);
        Scribe_Values.Look(ref lastFormUpTick, "lastFormUpTick", -1);
        Scribe_Values.Look(ref lastPunishedTick, "lastPunishedTick", -1);
        Scribe_Collections.Look(ref history, "history", LookMode.Deep);

        if (Scribe.mode == LoadSaveMode.LoadingVars && history == null)
            history = new List<RankHistoryEntry>();
    }
}
