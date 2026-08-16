namespace FCP.Ranks;

public class RankExpeditionDef : Def
{
    public IntRange durationDays;
    public List<SkillGain> xpReward;
    public ThingSetMakerDef lootMaker;
    public FloatRange lootMarketValueRange;
    public float siteRevealChance;
    public List<string> returnFlavorKeys;
    public float failureChance;
}
