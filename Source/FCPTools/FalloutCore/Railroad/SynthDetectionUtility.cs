namespace FCP.Railroad;

public static class SynthDetectionUtility
{
    public static IEnumerable<Pawn> GetHiddenSynthColonists(Map map)
    {
        if (RailroadDefOf.FCP_Hediff_SynthComponents_GenIII == null)
            yield break;

        foreach (Pawn pawn in map.mapPawns.FreeColonists)
        {
            if (pawn.health.hediffSet.HasHediff(RailroadDefOf.FCP_Hediff_SynthComponents_GenIII))
                yield return pawn;
        }
    }
}
