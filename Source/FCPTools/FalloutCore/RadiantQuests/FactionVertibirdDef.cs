using Verse;

namespace FCP.Core.RadiantQuests;

public class FactionExtension_Vertibird : DefModExtension
{
    public ThingDef crashedVertibird;
    public List<SurvivorGroupConfig> survivorGroups;
}

public class SurvivorGroupConfig
{
    public PawnKindDef leaderKind;
    public PawnKindDef basicKind;
    public int count = 8;
    public float difficultyMultiplier = 1f;
}
