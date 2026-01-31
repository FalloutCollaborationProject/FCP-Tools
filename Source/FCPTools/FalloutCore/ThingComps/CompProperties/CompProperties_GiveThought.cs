namespace FCP.Core;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class CompProperties_GiveThought : CompProperties
{
    public ThoughtDef thoughtDef;
    public int radius;
    public bool enableInInventory;

    public CompProperties_GiveThought() => compClass = typeof(CompGiveThought);
}