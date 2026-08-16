namespace FCP.Ranks;

public class RankTierDef : Def
{
    public string iconPath;
    public string pathName;
    public string discipline;
    public int minDaysInRank;
    public List<SkillRequirement> skillRequirements;
    public int minSquiresTrained;
    public int minMissionsCompleted;
    public HediffDef grantHediff;
    public List<SkillGain> trainingXpPerHour;
    public RankExpeditionDef expedition;
    public RoyalTitleDef correspondingRoyalTitle;
    public PreceptDef correspondingIdeoRole;
    public List<RankTierDef> nextTierOptions;
    public bool isCommanderTier;
    public float auraRadius = 16f;
    public HediffDef auraHediff;
    public HediffDef rallyHediff;
    public int rallyCooldownDays = 2;
}
