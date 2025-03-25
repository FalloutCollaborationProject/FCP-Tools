using Verse;
using Verse.AI.Group;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FCP.Core
{
    public class IncidentWorker_MercenaryArrival : IncidentWorker
    {
        private const float MinGoodwillToArrive = 0f;

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            if (!base.CanFireNowSub(parms)) return false;

            Map map = (Map)parms.target;
            if (map == null) return false;
            if (map.lordManager.lords.Any(l => l.LordJob is LordJob_MercenaryCamp))
            {
                return false;
            }
            if (!TryFindMercenaryFaction(out _, map))
            {
                return false;
            }

            return true;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            if (map == null) return false;

            if (!TryFindMercenaryFaction(out Faction faction, map)) return false;

            var extension = faction.def.GetModExtension<MercenaryExtension>();
            if (extension.mercenaryGroupToArrive == null)
            {
                Log.Error($"FCP: Mercenary faction {faction.Name} is missing mercenaryGroupToArrive.");
                return false;
            }
            PawnGroupMakerParms groupMakerParms = new PawnGroupMakerParms
            {
                groupKind = PawnGroupKindDefOf.Combat,
                tile = map.Tile,
                faction = faction,
                dontUseSingleUseRocketLaunchers = true
            };
            groupMakerParms.points = StorytellerUtility.DefaultThreatPointsNow(map);
            var minPoints = Mathf.Max(300, faction.def.MinPointsToGeneratePawnGroup(groupMakerParms.groupKind));
            if (parms.points < minPoints && minPoints < float.MaxValue)
            {
                groupMakerParms.points = minPoints;
            }

            // Safely generate pawns with additional null checks
            List<Pawn> pawns = new List<Pawn>();
            try
            {
                if (extension.mercenaryGroupToArrive.options != null && extension.mercenaryGroupToArrive.options.Any())
                {
                    pawns = extension.mercenaryGroupToArrive.GeneratePawns(groupMakerParms).ToList();
                }
                else
                {
                    // Fallback to a standard combat group if mercenary group has no options
                    PawnGroupMaker fallbackGroupMaker = faction.def.pawnGroupMakers.FirstOrDefault(pgm => pgm.kindDef == PawnGroupKindDefOf.Combat);
                    if (fallbackGroupMaker != null)
                    {
                        pawns = fallbackGroupMaker.GeneratePawns(groupMakerParms).ToList();
                        Log.Warning($"FCP: Using fallback combat group for {faction.Name} as mercenary group had no options.");
                    }
                    else
                    {
                        Log.Error($"FCP: Could not find any valid pawn group maker for {faction.Name}.");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"FCP: Error generating pawns for mercenary group: {ex}");
            }

            if (!pawns.Any())
            {
                Log.Error($"FCP: Failed to generate any pawns for mercenary group {faction.Name}.");
                return false;
            }

            if (!RCellFinder.TryFindRandomPawnEntryCell(out IntVec3 arrivalSpot, map, CellFinder.EdgeRoadChance_Neutral))
            {
                Log.Error($"FCP: Could not find arrival spot for mercenary group {faction.Name} on map {map.Index}.");
                return false;
            }

            foreach (Pawn pawn in pawns)
            {
                GenSpawn.Spawn(pawn, arrivalSpot, map, WipeMode.Vanish);
            }
            // Find a suitable camp center location
            IntVec3 campCenter;
            if (!TryFindCampCenter(map, faction, arrivalSpot, out campCenter))
            {
                // If TryFindCampCenter returns false, it still sets campCenter to a valid position (the arrival spot)
                Log.Warning($"FCP: Could not find ideal camp center for {faction.Name} on map {map.Index}. Using arrival spot at {campCenter}.");
            }

            // Double-check that the camp center is valid
            if (!campCenter.InBounds(map))
            {
                Log.Error($"FCP: Camp center {campCenter} is out of bounds. Using arrival spot instead.");
                campCenter = arrivalSpot;
            }

            Log.Message($"FCP: Final camp center for {faction.Name} at {campCenter} on map {map.Index}.");

            LordJob lordJob = new LordJob_MercenaryCamp(faction, campCenter);
            LordMaker.MakeNewLord(faction, lordJob, map, pawns);
            TaggedString letterLabel = "LetterLabelMercenaryGroupArrival".Translate(faction.Named("FACTION"));
            TaggedString letterText = "LetterMercenaryGroupArrival".Translate(faction.Named("FACTION"));
            TaggedString relationsInfo = "LetterRelatedPawnsNeutralGroup".Translate(Faction.OfPlayer.def.pawnsPlural);
            PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter(pawns, ref letterLabel, ref letterText, relationsInfo, true, true);
            Find.LetterStack.ReceiveLetter(letterLabel, letterText, LetterDefOf.NeutralEvent, pawns[0]);

            return true;
        }

        private bool TryFindMercenaryFaction(out Faction faction, Map map)
        {
            faction = Find.FactionManager.AllFactionsVisible
                .Where(f => f.def.HasModExtension<MercenaryExtension>() &&
                            !f.IsPlayer &&
                            !f.HostileTo(Faction.OfPlayer) &&
                            f.GoodwillWith(Faction.OfPlayer) >= MinGoodwillToArrive)
                .RandomElementWithFallback();

            return faction != null;
        }


        private bool TryFindCampCenter(Map map, Faction faction, IntVec3 nearSpot, out IntVec3 result)
        {
            // Initialize with a valid position (the arrival spot) as a fallback
            result = nearSpot;

            if (map == null)
            {
                Log.Error("FCP: Map is null in TryFindCampCenter");
                return false;
            }

            if (!nearSpot.InBounds(map))
            {
                Log.Error($"FCP: Near spot {nearSpot} is out of bounds in TryFindCampCenter");
                // Try to find a valid spot within the map
                nearSpot = CellFinder.RandomCell(map);
            }

            var extension = faction?.def?.GetModExtension<MercenaryExtension>();
            float campRadius = extension?.mercenaryCampRadius ?? 20f;

            // Make sure we respect the camp radius when placing near edges
            // We need a buffer zone of at least the camp radius from the map edge
            int edgeBuffer = Mathf.CeilToInt(campRadius);

            // Define a predicate that checks if a cell is valid for camp placement
            Predicate<IntVec3> validator = c =>
            {
                // Check if the cell is valid
                if (!IsValidCampCell(c, map))
                    return false;

                // Check if the cell is far enough from map edges
                if (c.CloseToEdge(map, edgeBuffer))
                    return false;
                return true;
            };

            // First priority: Try to find a cell near the arrival spot
            try
            {
                // Look for a suitable cell within a reasonable radius of the arrival spot
                int searchRadius = Mathf.RoundToInt(campRadius * 1.5f);
                if (CellFinder.TryFindRandomCellNear(nearSpot, map, searchRadius, validator, out IntVec3 nearCell))
                {
                    result = nearCell;
                    Log.Message($"FCP: Found camp center near arrival spot at {result}");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"FCP: Error finding camp center near arrival spot: {ex.Message}");
            }

            // Second priority: Try to find a cell in a more open area of the map
            try
            {
                // Try to find a cell near the center of the map
                IntVec3 mapCenter = map.Center;
                int searchRadius = Mathf.RoundToInt(map.Size.x / 4); // Use 1/4 of map width as search radius
                if (CellFinder.TryFindRandomCellNear(mapCenter, map, searchRadius,
                    validator, out IntVec3 centerCell))
                {
                    result = centerCell;
                    Log.Message($"FCP: Found camp center near map center at {result}");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"FCP: Error finding camp center near map center: {ex.Message}");
            }

            // Third priority: Try to find any valid cell on the map
            try
            {
                // Try to find any valid cell on the map
                if (CellFinder.TryFindRandomCell(map, validator, out IntVec3 anyCell))
                {
                    result = anyCell;
                    Log.Message($"FCP: Found camp center at random location {result}");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"FCP: Error finding random camp center: {ex.Message}");
            }

            // If all else fails, just use the arrival spot if it's valid
            // But first check if it's far enough from the edges
            if (!nearSpot.CloseToEdge(map, edgeBuffer) && IsValidCampCell(nearSpot, map))
            {
                Log.Message($"FCP: Using arrival spot as camp center at {nearSpot}");
                return true;
            }

            // Last resort: Try to find a spot that's at least in bounds
            try
            {
                IntVec3 safeSpot = CellFinder.RandomCell(map);
                // Try to move it away from edges if possible
                safeSpot.x = Mathf.Clamp(safeSpot.x, edgeBuffer, map.Size.x - edgeBuffer);
                safeSpot.z = Mathf.Clamp(safeSpot.z, edgeBuffer, map.Size.z - edgeBuffer);

                if (safeSpot.InBounds(map))
                {
                    result = safeSpot;
                    Log.Warning($"FCP: Using last resort safe spot as camp center at {result}");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"FCP: Error finding last resort camp center: {ex.Message}");
            }

            Log.Warning($"FCP: Could not find any valid camp center. Using arrival spot as fallback at {nearSpot}");
            return false;
        }

        private bool IsValidCampCell(IntVec3 cell, Map map)
        {
            try
            {
                // Basic validity checks
                if (map == null || !cell.InBounds(map))
                    return false;

                if (!cell.Standable(map) || cell.GetTerrain(map).IsWater || map.roofGrid.Roofed(cell))
                    return false;

                // Check for player buildings nearby
                foreach (var thing in map.listerThings.ThingsInGroup(ThingRequestGroup.BuildingArtificial))
                {
                    if (thing != null && thing.Faction == Faction.OfPlayer && thing.Position.InHorDistOf(cell, 15f))
                    {
                        return false;
                    }
                }

                // Check for walkability in the area around the cell
                // This ensures the camp has enough space to function
                int walkableRadius = 5; // Smaller check radius for performance
                int walkableCellsRequired = Mathf.RoundToInt(walkableRadius * walkableRadius * 0.7f); // Require ~70% of cells to be walkable
                int walkableCellsFound = 0;

                foreach (IntVec3 c in GenRadial.RadialCellsAround(cell, walkableRadius, true))
                {
                    if (c.InBounds(map) && c.Walkable(map))
                    {
                        walkableCellsFound++;
                        if (walkableCellsFound >= walkableCellsRequired)
                            break;
                    }
                }

                if (walkableCellsFound < walkableCellsRequired)
                    return false;

                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"FCP: Error in IsValidCampCell: {ex.Message}");
                return false;
            }
        }
    }
}
