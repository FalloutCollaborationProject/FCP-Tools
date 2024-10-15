using UnityEngine;

namespace FCP.Core;

/// <summary>
/// Generic version of the Thrumbo Passes incident.
/// Uses ModExtension_AnimalPassesConfig as Configuration.
/// Steve, changed Config to _config to conform to C# naming conventions lol
/// </summary>
public class IncidentWorker_AnimalPasses : IncidentWorker
{
    private ModExtension_AnimalPassesConfig _config => def.GetModExtension<ModExtension_AnimalPassesConfig>();
        
    protected override bool CanFireNowSub(IncidentParms parms)
    {
        Map map = (Map)parms.target;

        bool toxicFalloutActive = !_config.ignoreToxicFallout && map.gameConditionManager.ConditionIsActive(GameConditionDefOf.ToxicFallout);
        bool noxiousHazeActive = ModsConfig.BiotechActive && map.gameConditionManager.ConditionIsActive(GameConditionDefOf.NoxiousHaze);
        bool temperatureUnacceptable = !_config.ignoreTemperature && !map.mapTemperature.SeasonAndOutdoorTemperatureAcceptableFor(_config.animalThing);

        if (toxicFalloutActive || noxiousHazeActive || temperatureUnacceptable)
        {
            return false;
        }
            
        return TryFindEntryCell(map, out _);
    }

    protected override bool TryExecuteWorker(IncidentParms parms)
    {
        Map map = (Map)parms.target;
        if (!TryFindEntryCell(map, out IntVec3 cell)) return false;
            
        PawnKindDef pawnKindDef = _config.animalPawnKind;
        int animalCount = GenMath.RoundRandom(StorytellerUtility.DefaultThreatPointsNow(map) / pawnKindDef.combatPower);
        animalCount = Mathf.Clamp(animalCount, min: _config.minCount, max: _config.maxRangeInclusive.RandomInRange);

        if (!RCellFinder.TryFindRandomCellOutsideColonyNearTheCenterOfTheMap(cell, map, 10f, out IntVec3 forcedGotoPosition))
        {
            forcedGotoPosition = IntVec3.Invalid;
        }
            
        Pawn generatedPawn = null;
        for (int i = 0; i < animalCount; i++)
        {
            // Create and spawn the pawns
            IntVec3 enterCell = CellFinder.RandomClosewalkCellNear(cell, map, 10);
                
            generatedPawn = PawnGenerator.GeneratePawn(pawnKindDef);
            GenSpawn.Spawn(generatedPawn, enterCell, map, Rot4.Random);
                
            // Leave the map after a random amount of ticks.
            generatedPawn.mindState.exitMapAfterTick = Find.TickManager.TicksGame + _config.ticksToLeave.RandomInRange;
            if (forcedGotoPosition.IsValid)
            {
                generatedPawn.mindState.forcedGotoPosition = CellFinder.RandomClosewalkCellNear(forcedGotoPosition, map, 10);
            }
        }
            
        SendStandardLetter(_config.letterLabel.Translate(), _config.letterText.Translate(), LetterDefOf.PositiveEvent, parms, generatedPawn);
        return true;
    }

    protected virtual bool TryFindEntryCell(Map map, out IntVec3 cell)
    {
        return RCellFinder.TryFindRandomPawnEntryCell(out cell, map, CellFinder.EdgeRoadChance_Animal + 0.2f);
    }
}