using HarmonyLib;

namespace FCP.Core.Birds;

[StaticConstructorOnStartup]
public static class BirdUtils
{
    private static Dictionary<Pawn, CompFlyingPawn> cachedComps = new Dictionary<Pawn, CompFlyingPawn>();

    static BirdUtils()
    {
        new Harmony("FCP.Core.Birds").PatchCategory("Birds");
    }

    public static bool IsFlyingPawn(this Pawn pawn, out CompFlyingPawn comp)
    {
        comp = null;
        
        if (pawn == null)
            return false;

        cachedComps ??= [];

        if (!cachedComps.TryGetValue(pawn, out comp))
        {
            cachedComps[pawn] = pawn.TryGetComp<CompFlyingPawn>();
        }

        comp = cachedComps[pawn];
        return comp != null;
    }
}