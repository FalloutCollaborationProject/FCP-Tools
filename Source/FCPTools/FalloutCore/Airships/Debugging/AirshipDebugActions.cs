using FCP.Factions;
using LudeonTK;
using RimWorld.Planet;

namespace FCP.Core;

public static class AirshipDebugActions
{
    private const string CategoryName = "FCP: Airships";

    private static (Airship airship, List<WorldObject> settlements) SpawnBoSAirship()
    {
        var airship = (Airship)WorldObjectMaker.MakeWorldObject(FCPAirshipDefOf.FCP_BOS_Airship);

        // Todo DefOf
        FactionGroupDef bosFactions = DefDatabase<FactionGroupDef>.GetNamed("FCP_Factions_BoS");
        Faction bosFaction = Find.FactionManager.FirstFactionOfDef(bosFactions.leadingFaction);
        airship.SetFaction(bosFaction);

        List<WorldObject> settlements = Find.WorldObjects.Settlements
            .Where(s => bosFactions.factions.Contains(s.Faction.def))
            .TakeRandomDistinct(4)
            .Cast<WorldObject>()
            .ToList();

        airship.Tile = settlements.First().Tile;
        Find.WorldObjects.Add(airship);

        return (airship, settlements);
    }

    [DebugAction(CategoryName, "Spawn Airship (Idle)", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.Playing)]
    private static void SpawnAirshipIdle()
    {
        (Airship airship, List<WorldObject> settlements) = SpawnBoSAirship();

        FCPLog.Message($"Airship spawned idle at {settlements.First().Label}");
        Messages.Message($"Airship spawned idle at {settlements.First().Label}", airship, MessageTypeDefOf.NeutralEvent);
    }

    [DebugAction(CategoryName, "Spawn Airship (Route)", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.Playing)]
    private static List<DebugActionNode> SpawnAirshipWithRoute()
    {
        return
        [
            new DebugActionNode("Patrol Route", DebugActionType.Action)
            {
                action = () =>
                {
                    (Airship airship, List<WorldObject> settlements) = SpawnBoSAirship();
                    
                    var route = new PatrolRoute(settlements);
                    BuildLegs(route, settlements);
                    airship.AssignRoute(route);
                    NotifySpawned(airship, "patrol");
                }
            },
            new DebugActionNode("Trade Route", DebugActionType.Action)
            {
                action = () =>
                {
                    (Airship airship, List<WorldObject> settlements) = SpawnBoSAirship();
                    
                    WorldObject tradeTarget = settlements.Last();
                    var route = new TradeRoute(tradeTarget);
                    BuildLegs(route, settlements);
                    airship.AssignRoute(route);
                    NotifySpawned(airship, "trade");
                }
            },
            new DebugActionNode("Direct Route", DebugActionType.Action)
            {
                action = () =>
                {
                    (Airship airship, List<WorldObject> settlements) = SpawnBoSAirship();
                    
                    var route = new DirectRoute();
                    BuildLegs(route, settlements);
                    airship.AssignRoute(route);
                    NotifySpawned(airship, "direct");
                }
            }
        ];
    }

    private static void BuildLegs(AirshipRoute route, List<WorldObject> stops)
    {
        for (int i = 0; i < stops.Count - 1; i++)
            route.AddLeg(stops[i], stops[i + 1]);
    }

    private static void NotifySpawned(Airship airship, string routeType)
    {
        string msg = $"Airship spawned with {routeType} route";
        FCPLog.Message(msg);
        ChoiceLetter letter = LetterMaker.MakeLetter("Airship", msg, LetterDefOf.NeutralEvent, airship);
        Find.LetterStack.ReceiveLetter(letter);
        Messages.Message(msg, airship, MessageTypeDefOf.NeutralEvent);
    }
}
