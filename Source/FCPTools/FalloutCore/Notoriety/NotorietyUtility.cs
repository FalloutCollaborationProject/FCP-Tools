namespace FCP.Core;

public static class NotorietyUtility
{
    private static readonly HistoryEventDef[] TrackedEvents =
    {
        HistoryEventDefOf.AteHumanMeatDirect,
        HistoryEventDefOf.ButcheredHuman,
        HistoryEventDefOf.ExecutedPrisonerGuilty,
        HistoryEventDefOf.ExecutedPrisonerInnocent,
        HistoryEventDefOf.ExecutedGuest,
        HistoryEventDefOf.EnslavedPrisoner
    };

    public static int GetNotoriety(int windowTicks = GenDate.TicksPerSeason)
    {
        HistoryEventsManager manager = Find.HistoryEventsManager;
        int total = 0;
        foreach (HistoryEventDef eventDef in TrackedEvents)
            total += manager.GetRecentCountWithinTicks(eventDef, windowTicks, Faction.OfPlayer);
        return total;
    }

    public static bool IsNotorious(int threshold = 5) => GetNotoriety() >= threshold;
}
