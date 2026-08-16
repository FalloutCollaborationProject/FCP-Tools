namespace FCP.Ranks;

public class ExpeditionData : IExposable
{
    public RankExpeditionDef expedition;
    public int returnTick;
    public Map originMap;

    public void ExposeData()
    {
        Scribe_Defs.Look(ref expedition, "expedition");
        Scribe_Values.Look(ref returnTick, "returnTick");
        Scribe_References.Look(ref originMap, "originMap");
    }
}
