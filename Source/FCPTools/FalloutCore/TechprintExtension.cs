namespace FCP.Core;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class TechprintExtension : DefModExtension
{
    public string baseLabel;
    public string baseDescription;
    public string texPath;
    public List<ThingDef> requiredBenches;
}