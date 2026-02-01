namespace FCP.Core;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class RecipeExtension_GoodwillCheck : DefModExtension
{
    public FactionDef requireFaction;

    public int minimumGoodwill = -1;

    public bool uncraftableIfFactionDefeated = true;
}