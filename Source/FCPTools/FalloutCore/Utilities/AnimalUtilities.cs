namespace FCP.Core;

public static class AnimalUtilities
{
    public static bool IsFlyingPawn(this Pawn pawn, out CompFlyingPawn comp)
    {
        comp = pawn?.TryGetComp<CompFlyingPawn>();
        return comp != null;
    }
}