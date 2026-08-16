namespace FCP.Ranks;

public class RankHistoryEntry : IExposable
{
    public int tick;
    public string text;

    public void ExposeData()
    {
        Scribe_Values.Look(ref tick, "tick");
        Scribe_Values.Look(ref text, "text");
    }
}
