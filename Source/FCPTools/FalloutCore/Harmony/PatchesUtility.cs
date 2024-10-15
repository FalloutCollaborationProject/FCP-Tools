namespace FCP.Core;

public static class PatchesUtility
{
    public static bool CanRecruit(Pawn pawn)
    {
        ModExtension_PawnKindProperties props = ModExtension_PawnKindProperties.Get(pawn.kindDef);
        return props is { purchasableFromTrader: true };
    }
}