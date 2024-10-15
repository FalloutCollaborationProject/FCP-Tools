namespace FCP.Core;

/// <summary>
/// Added Utility for defining if a pawn is purchasable from a trader.
/// </summary>
public class ModExtension_PawnKindProperties : DefModExtension
{
    public bool purchasableFromTrader = false;

    public static ModExtension_PawnKindProperties Get(Def def)
    {
        return def.GetModExtension<ModExtension_PawnKindProperties>();
    }
}