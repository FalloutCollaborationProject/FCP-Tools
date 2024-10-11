using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using RimWorld;
using UnityEngine;
using Verse;

namespace FCP.Core;

/// <summary>
/// Configuration ModExtension for the IncidentWorker_AnimalPasses
/// </summary>
[UsedImplicitly, SuppressMessage("ReSharper", "UnassignedField.Global")]
public class AnimalPassesConfig : DefModExtension
{
    public string letterLabel;
    public string letterText;
    
    public ThingDef animalThing;
    public PawnKindDef animalPawnKind;

    public int minCount = 1;
    public IntRange maxRangeInclusive = new IntRange(3, 6);

    public IntRange ticksToLeave = new IntRange(90000, 150000);

    public bool ignoreTemperature = false;
    public bool ignoreToxicFallout = false;

    public override IEnumerable<string> ConfigErrors()
    {
        if (animalThing == null)
            yield return "animalThing in a AnimalPassesConfig extension is null.";
        
        if (animalPawnKind == null)
            yield return "animalPawnKind in a AnimalPassesConfig extension is null.";
    }
}

/// <summary>
/// Generic version of the Thrumbo Passes incident, using the AnimalPassesConfig Extension as Configuration.
/// </summary>
public class IncidentWorker_AnimalPasses : IncidentWorker
{
    protected AnimalPassesConfig Config => def.GetModExtension<AnimalPassesConfig>();
    
    protected override bool CanFireNowSub(IncidentParms parms)
    {
        var map = (Map)parms.target;

        if (!Config.ignoreToxicFallout && map.gameConditionManager.ConditionIsActive(GameConditionDefOf.ToxicFallout))
        {
            return false;
        }
        if (ModsConfig.BiotechActive && map.gameConditionManager.ConditionIsActive(GameConditionDefOf.NoxiousHaze))
        {
            return false;
        }
        if (!Config.ignoreTemperature && !map.mapTemperature.SeasonAndOutdoorTemperatureAcceptableFor(Config.animalThing))
        {
            return false;
        }

        return TryFindEntryCell(map, out _);
    }

    protected override bool TryExecuteWorker(IncidentParms parms)
    {
        var map = (Map)parms.target;
        if (!TryFindEntryCell(map, out var cell))
            return false;
        
        var pawnKindDef = Config.animalPawnKind;
        int animalCount = GenMath.RoundRandom(StorytellerUtility.DefaultThreatPointsNow(map) / pawnKindDef.combatPower);
        animalCount = Mathf.Clamp(animalCount, min: Config.minCount, max: Config.maxRangeInclusive.RandomInRange);

        if (!RCellFinder.TryFindRandomCellOutsideColonyNearTheCenterOfTheMap(cell, map, 10f, out IntVec3 forcedGotoPosition))
        {
            forcedGotoPosition = IntVec3.Invalid;
        }
        
        Pawn generatedPawn = null;
        for (int i = 0; i < animalCount; i++)
        {
            // Create and spawn the pawns
            var enterCell = CellFinder.RandomClosewalkCellNear(cell, map, 10);
            
            generatedPawn = PawnGenerator.GeneratePawn(pawnKindDef);
            GenSpawn.Spawn(generatedPawn, enterCell, map, Rot4.Random);
            
            // Leave the map after a random amount of ticks.
            generatedPawn.mindState.exitMapAfterTick = Find.TickManager.TicksGame + Config.ticksToLeave.RandomInRange;
            if (forcedGotoPosition.IsValid)
            {
                generatedPawn.mindState.forcedGotoPosition = CellFinder.RandomClosewalkCellNear(forcedGotoPosition, map, 10);
            }
        }
        
        SendStandardLetter(Config.letterLabel.Translate(), Config.letterText.Translate(), LetterDefOf.PositiveEvent, parms, generatedPawn);
        return true;
    }

    protected virtual bool TryFindEntryCell(Map map, out IntVec3 cell)
    {
        return RCellFinder.TryFindRandomPawnEntryCell(out cell, map, CellFinder.EdgeRoadChance_Animal + 0.2f);
    }
}