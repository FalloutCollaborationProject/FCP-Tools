namespace FCP.Core.Abolitionists;

public static class AbolitionistsUtility
{
    private static FactionDef factionDefCached;

    private static FactionDef FactionDef => factionDefCached ??= DefDatabase<FactionDef>.GetNamedSilentFail("FCP_Faction_Abolitionists");

    public static Faction GetFaction()
    {
        FactionDef def = FactionDef;
        return def == null ? null : Find.FactionManager.FirstFactionOfDef(def);
    }

    public static bool ColonyOwnsSlaves()
    {
        List<Map> maps = Find.Maps;
        for (int i = 0; i < maps.Count; i++)
        {
            Map map = maps[i];
            if (map.IsPlayerHome && map.mapPawns.SlavesOfColonySpawned.Count > 0)
            {
                return true;
            }
        }

        return false;
    }

    public static void RewardSlaveRelease(Pawn freedSlave)
    {
        Faction abolitionists = GetFaction();
        if (abolitionists == null)
        {
            return;
        }

        Faction player = Faction.OfPlayer;
        if (abolitionists.PlayerGoodwill < 100)
        {
            abolitionists.TryAffectGoodwillWith(player, Rand.RangeInclusive(8, 15), canSendMessage: false, canSendHostilityLetter: false, reason: null, lookTarget: null);
        }

        Map map = freedSlave.MapHeld;
        if (map != null)
        {
            Thing caps = ThingMaker.MakeThing(ThingDefOf_Abolitionists.FCP_Currency_Caps);
            caps.stackCount = Rand.RangeInclusive(20, 60);
            GenPlace.TryPlaceThing(caps, freedSlave.PositionHeld, map, ThingPlaceMode.Near);
        }

        Find.LetterStack.ReceiveLetter(
            "FCP_Abolitionist_ReleaseLetterLabel".Translate(),
            "FCP_Abolitionist_ReleaseLetterText".Translate(freedSlave.Named("PAWN")),
            LetterDefOf.PositiveEvent,
            freedSlave);
    }
}

[DefOf]
public static class ThingDefOf_Abolitionists
{
    public static ThingDef FCP_Currency_Caps;
}
