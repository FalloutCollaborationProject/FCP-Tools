namespace FCP.Core
{
    public static class AnimalUtilities
    {
        private static Dictionary<Pawn, CompFlyingPawn> cachedComps = new ();
        
        public static bool IsFlyingPawn(this Pawn pawn, out CompFlyingPawn comp)
        {
            comp = null;
        
            if (pawn == null) return false;
            cachedComps ??= [];

            if (!cachedComps.TryGetValue(pawn, out comp))
            {
                cachedComps[pawn] = pawn.TryGetComp<CompFlyingPawn>();
            }

            comp = cachedComps[pawn];
            return comp != null;
        }
    }
}