namespace FCP.Core.Buildings;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class CompProperties_TerminalHacking : CompProperties
{
    public List<ThingDef> rewardPool;
    public int wordLength = 6;
    public int maxAttempts = 4;
    public int lockoutDurationHours = 6;
    public int wordCount = 12;

    public CompProperties_TerminalHacking()
    {
        compClass = typeof(CompTerminalHacking);
    }
}