using UnityEngine;

namespace FCP.Core;

/// <summary>
/// Generic version of the Thrumbo Passes incident.
/// Uses ModExtension_AnimalPassesConfig as Configuration.
/// </summary>
public class IncidentWorker_AnimalPasses : IncidentWorker
{
    private ModExtension_AnimalPassesConfig Config =>
        def.GetModExtension<ModExtension_AnimalPassesConfig>();

    protected override bool CanFireNowSub(IncidentParms parms)
    {
        if (parms.target is not Map map) 
            return false;

        bool toxicFalloutActive = !Config.ignoreToxicFallout &&
                                  map.gameConditionManager.ConditionIsActive(GameConditionDefOf.ToxicFallout);

        bool noxiousHazeActive = ModsConfig.BiotechActive &&
                                 map.gameConditionManager.ConditionIsActive(GameConditionDefOf.NoxiousHaze);

        bool temperatureUnacceptable = !Config.ignoreTemperature &&
                                       !map.mapTemperature.SeasonAndOutdoorTemperatureAcceptableFor(Config.animalThing);

        return !toxicFalloutActive &&
               !noxiousHazeActive &&
               !temperatureUnacceptable &&
               TryFindEntryCell(map, out _);
    }

    protected override bool TryExecuteWorker(IncidentParms parms)
    {
        if (parms.target is not Map map)
            return false;
        
        if (!TryFindEntryCell(map, out IntVec3 cell))
            return false;

        PawnKindDef animalPawnKind = Config.animalPawnKind;

        int animalCount = GenMath.RoundRandom(StorytellerUtility
            .DefaultThreatPointsNow(map) / animalPawnKind.combatPower);

        animalCount = Mathf.Clamp(animalCount, 
            min: Config.minCount,
            max: Config.maxRangeInclusive.RandomInRange);

        if (!RCellFinder.TryFindRandomCellOutsideColonyNearTheCenterOfTheMap(
                cell, map, 10f, out IntVec3 forcedGotoPosition))
        {
            forcedGotoPosition = IntVec3.Invalid;
        }

        Pawn generatedPawn = null;
        for (int i = 0; i < animalCount; i++)
        {
            // create and spawn the pawns
            IntVec3 enterCell = CellFinder.RandomClosewalkCellNear(cell, map, 10);

            generatedPawn = PawnGenerator.GeneratePawn(animalPawnKind);
            GenSpawn.Spawn(generatedPawn, enterCell, map, Rot4.Random);

            // leave the map after a random amount of ticks.
            generatedPawn.mindState.exitMapAfterTick = Find.TickManager.TicksGame +
                                                       Config.ticksToLeave.RandomInRange;
            if (forcedGotoPosition.IsValid)
            {
                generatedPawn.mindState.forcedGotoPosition =
                    CellFinder.RandomClosewalkCellNear(forcedGotoPosition, map, 10);
            }
        }

        SendStandardLetter(def.letterLabel.Translate(), def.letterText.Translate(),
            baseLetterDef: LetterDefOf.PositiveEvent, 
            parms: parms, 
            lookTargets: generatedPawn);
        
        return true;
    }

    protected virtual bool TryFindEntryCell(Map map, out IntVec3 cell)
    {
        return RCellFinder.TryFindRandomPawnEntryCell(out cell, map, CellFinder.EdgeRoadChance_Animal + 0.2f);
    }
}