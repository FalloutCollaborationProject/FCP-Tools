namespace FCP.Synths;

public class SynthTierDef : Def
{
    public int tier;

    public PawnKindDef pawnKindDef;

    public ResearchProjectDef requiredResearch;

    public List<ThingDefCountClass> ingredients = new();

    public float gestationDays = 1f;

    public float powerDrawIdleWatts = 50f;

    public float powerDrawActiveWatts = 400f;

    public HediffDef bodyOverrideHediff;

    public HediffDef yieldHediff;
}
