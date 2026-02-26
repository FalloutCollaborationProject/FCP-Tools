using FCP.Factions;
using LudeonTK;
using RimWorld.Planet;

namespace FCP.Core;

public static class AirshipDebugActions
{
    private const string CategoryName = "FCP: Airships";

    [DebugAction(CategoryName, "Spawn Airship", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.Playing)]
    private static void SpawnAirship()
    {
        FCPLog.Message("Spawning Airship");
        var airship = (Airship)WorldObjectMaker.MakeWorldObject(FCPAirshipDefOf.FCP_BOS_Airship);

        // Todo DefOf
        FactionGroupDef bosFactions = DefDatabase<FactionGroupDef>.GetNamed("FCP_Factions_BoS");
        Faction bosFaction = Find.FactionManager.FirstFactionOfDef(bosFactions.leadingFaction);
        airship.SetFaction(bosFaction);
        
        // Set up route through first 4 settlements
        var settlements = Find.WorldObjects.Settlements
            .Where(settlement => bosFactions.factions.Contains(settlement.Faction.def))
            .TakeRandomDistinct(4);
        
        FCPLog.Message($"Settlements: {settlements.Select(x => x.Label).ToStringList(", ")}");
        
        FCPLog.Message("Setting Route");
        airship.SetRoute(settlements);
        
        Find.WorldObjects.Add(airship);
        FCPLog.Message("Starting Journey");
        airship.StartJourney();

        ChoiceLetter letter = LetterMaker.MakeLetter("Airship", "Airship route established", LetterDefOf.NeutralEvent, airship);
        Find.LetterStack.ReceiveLetter(letter);
        Messages.Message("Airship spawned and en route", airship, MessageTypeDefOf.NeutralEvent);
    }

}