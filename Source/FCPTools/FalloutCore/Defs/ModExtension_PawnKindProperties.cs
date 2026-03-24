namespace FCP.Core;

/// <summary>
/// Added Utility for defining if a pawn is purchasable from a trader.
/// </summary>
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