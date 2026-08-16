namespace FCP.Core;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class ModExtension_PawnKindProperties : DefModExtension
{
    public bool purchasableFromTrader = false;

    [CanBeNull]
    public static ModExtension_PawnKindProperties Get(Def def)
    {
        return def.GetModExtension<ModExtension_PawnKindProperties>();
    }
}