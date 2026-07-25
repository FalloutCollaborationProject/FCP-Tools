namespace FCP.Core.Radio;

public class CompProperties_Radio : CompProperties
{
    public List<RadioStationDef> stations = new List<RadioStationDef>();
    public List<RadioCaravanOption> caravanOptions = new List<RadioCaravanOption>();
    public int minGoodwillToCallReinforcements = 50;
    public int gatherIntelDurationTicks = 15000;
    public float gatherIntelSuccessChance = 0.2f;
    public int gatherIntelCooldownTicks = 300000;

    public CompProperties_Radio() => compClass = typeof(CompRadio);
}
