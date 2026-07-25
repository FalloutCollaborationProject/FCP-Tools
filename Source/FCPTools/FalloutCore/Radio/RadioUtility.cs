using FCP.Enlist;
using RimWorld.Planet;
using Verse.AI.Group;

namespace FCP.Core.Radio;

public static class RadioUtility
{
    public static bool CanSpawnNamedCaravan(CharacterDef characterDef, out string failReason)
    {
        UniqueCharactersTracker tracker = UniqueCharactersTracker.Instance;
        if (tracker.CharacterPawnDead(characterDef))
        {
            failReason = "FCP_Radio_TraderDead".Translate(characterDef.LabelCap);
            return false;
        }
        if (tracker.CharacterPawnSpawned(characterDef))
        {
            failReason = "FCP_Radio_TraderBusy".Translate(characterDef.LabelCap);
            return false;
        }
        failReason = null;
        return true;
    }

    public static bool TrySpawnNamedCaravan(Map map, CharacterDef characterDef, TraderKindDef traderKindOverride, out string failReason)
    {
        if (!CanSpawnNamedCaravan(characterDef, out failReason))
            return false;

        UniqueCharactersTracker tracker = UniqueCharactersTracker.Instance;
        Faction faction = Find.FactionManager.FirstFactionOfDef(characterDef.faction);
        Pawn trader = tracker.GetOrGenPawn(characterDef, null, faction);
        trader.mindState.wantsToTradeWithColony = true;
        PawnComponentsUtility.AddAndRemoveDynamicComponents(trader, true);
        if (traderKindOverride != null)
            trader.trader.traderKind = traderKindOverride;

        List<Pawn> pawns = new List<Pawn> { trader };
        IncidentParms parms = new IncidentParms { target = map, faction = faction };
        PawnsArrivalModeDefOf.EdgeWalkIn.Worker.Arrive(pawns, parms);

        if (trader.needs?.food != null)
            trader.needs.food.CurLevel = trader.needs.food.MaxLevel;

        if (!RCellFinder.TryFindRandomSpotJustOutsideColony(trader.Position, map, trader, out IntVec3 chillSpot))
        {
            failReason = null;
            return true;
        }

        LordMaker.MakeNewLord(faction, new LordJob_TradeWithColony(faction, chillSpot), map, pawns);
        Find.LetterStack.ReceiveLetter("FCP_Radio_TraderArrivedTitle".Translate(),
            "FCP_Radio_TraderArrivedText".Translate(trader.Named("TRADER"), faction.Named("FACTION")),
            LetterDefOf.PositiveEvent, trader);

        failReason = null;
        return true;
    }

    public static IEnumerable<(Faction faction, FactionEnlistOptionsDef options)> EnlistedFactionsWithOptions()
    {
        WorldEnlistTracker tracker = WorldEnlistTracker.Instance;
        if (tracker?.factionOptionsContainer == null)
            yield break;

        foreach (var factionEntry in tracker.factionOptionsContainer)
        {
            foreach (var optionEntry in factionEntry.Value.factionsRecruiters)
            {
                if (optionEntry.Value && EnlistUtils.EnlistStates.TryGetValue(optionEntry.Key.defName, out bool enabled) && enabled)
                    yield return (factionEntry.Key, optionEntry.Key);
            }
        }
    }

    public static int CountOnMap(Map map, ThingDef currencyDef)
    {
        return map.listerThings.ThingsOfDef(currencyDef).Sum(t => t.stackCount);
    }

    public static bool TryConsumeCurrency(Map map, ThingDef currencyDef, int cost)
    {
        if (CountOnMap(map, currencyDef) < cost)
            return false;

        int remaining = cost;
        foreach (Thing thing in map.listerThings.ThingsOfDef(currencyDef).ToList())
        {
            if (remaining <= 0) break;
            int take = Math.Min(remaining, thing.stackCount);
            thing.SplitOff(take).Destroy();
            remaining -= take;
        }
        return true;
    }

    public static bool TryRevealNearbySite(Pawn pawn)
    {
        List<SitePartDef> siteParts = DefDatabase<SitePartDef>.AllDefsListForReading
            .Where(d => d.defName.StartsWith("FCP_Site"))
            .ToList();
        if (siteParts.NullOrEmpty())
            return false;

        Faction faction = Find.FactionManager.AllFactionsVisible
            .Where(f => !f.IsPlayer && !f.Hidden && !f.HostileTo(Faction.OfPlayer) && f.def.defName.StartsWith("FCP_Faction"))
            .RandomElementWithFallback();
        if (faction == null)
            return false;

        int tile = TileFinder.RandomSettlementTileFor(faction, false);
        if (tile < 0)
            return false;

        SitePartDef partDef = siteParts.RandomElement();
        Site site = (Site)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Site);
        site.Tile = tile;
        site.SetFaction(faction);
        site.parts.Add(new SitePart(site, partDef, partDef.Worker.GenerateDefaultParams(
            StorytellerUtility.DefaultSiteThreatPointsNow(), tile, faction)));
        Find.WorldObjects.Add(site);

        Find.LetterStack.ReceiveLetter("FCP_Radio_IntelFoundTitle".Translate(),
            "FCP_Radio_IntelFoundText".Translate(pawn.Named("PAWN"), site.Named("SITE")),
            LetterDefOf.PositiveEvent, site);
        return true;
    }
}
