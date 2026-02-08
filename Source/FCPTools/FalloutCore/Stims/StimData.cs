namespace FCP.Core.Stims;

[StaticConstructorOnStartup]
public static class StimData
{
    public static readonly Dictionary<BodyDef, int> TotalHpForRace = new Dictionary<BodyDef, int>();
    public const int AverageHitPoint = 1000;

    static StimData()
    {
        foreach (BodyDef def in DefDatabase<BodyDef>.AllDefs)
        {
            int hp = def.AllParts.Sum(part => part.def.hitPoints);
            TotalHpForRace.Add(def, hp);
        }
    }
}
