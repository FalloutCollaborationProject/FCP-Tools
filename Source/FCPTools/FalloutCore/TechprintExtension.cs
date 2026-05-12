namespace FCP.Core;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class TechprintExtension : DefModExtension
{
    public string baseLabel;
    public string baseDescription;
    public string texPath;
    public ThingDef requiredBench;
    public List<ThingDef> requiredBenches;

    public List<ThingDef> AllRequiredBenches
    {
        get
        {
            if (requiredBench == null)
                return requiredBenches;
            if (requiredBenches == null)
                return new List<ThingDef> { requiredBench };
            var combined = new List<ThingDef>(requiredBenches);
            if (!combined.Contains(requiredBench))
                combined.Add(requiredBench);
            return combined;
        }
    }
}