using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace FCP.Factions.Harmony;

[HarmonyPatch(typeof(World), nameof(World.FinalizeInit))]
static class Settlement_FinalizeInit_Patch
{
	static void Postfix()
	{
		if (Current.Game.tickManager.TicksGame > 0)
			return;

		WorldGrid grid = Find.WorldGrid;
		List<Settlement> allSettlements = Find.WorldObjects.Settlements;
		GameComponent_SettlementMapGenerators gameComp = GetOrCreateGameComponent();

		foreach (Faction faction in Find.FactionManager.AllFactionsListForReading)
		{
			FactionExtension_SettlementControl ext = faction.def.GetModExtension<FactionExtension_SettlementControl>();
			if (ext == null)
				continue;

			List<Settlement> factionSettlements = new List<Settlement>();
			for (int j = 0; j < allSettlements.Count; j++)
			{
				if (allSettlements[j].Faction == faction)
					factionSettlements.Add(allSettlements[j]);
			}

			if (factionSettlements.Count == 0)
				continue;

			if (ext.maxSettlements > 0 && factionSettlements.Count > ext.maxSettlements)
			{
				for (int j = ext.maxSettlements; j < factionSettlements.Count; j++)
					Find.WorldObjects.Remove(factionSettlements[j]);
				factionSettlements.RemoveRange(ext.maxSettlements, factionSettlements.Count - ext.maxSettlements);
			}

			if (ext.namedSettlements.NullOrEmpty())
				continue;

			for (int j = 0; j < factionSettlements.Count && j < ext.namedSettlements.Count; j++)
			{
				Settlement settlement = factionSettlements[j];
				NamedSettlement namedSettlement = ext.namedSettlements[j];

				if (!namedSettlement.name.NullOrEmpty())
					settlement.Name = namedSettlement.name;

				if (namedSettlement.prefab != null)
					gameComp.RegisterSettlementWithPrefab(settlement, namedSettlement.prefab, namedSettlement.forcedMapSize, namedSettlement.guaranteedPawnKinds, namedSettlement.guaranteedCharacters, namedSettlement.traders);
				else if (namedSettlement.mapGenerator != null)
					gameComp.RegisterSettlement(settlement, namedSettlement.mapGenerator, namedSettlement.forcedMapSize);

				if (namedSettlement.traders != null && namedSettlement.traders.Count > 0)
				{
					WorldObjectComp_SettlementTraders comp = settlement.GetComponent<WorldObjectComp_SettlementTraders>();
					if (comp == null)
					{
						comp = new WorldObjectComp_SettlementTraders();
						comp.parent = settlement;
						settlement.AllComps.Add(comp);
						comp.SetTraders(namedSettlement.traders);
					}
				}

				List<Hilliness> hilliness = namedSettlement.preferredHilliness ?? ext.preferredHilliness;
				List<BiomeDef> biomes = namedSettlement.allowedBiomes;
				
				bool needsRelocation = false;
				Tile settlementTile = grid[settlement.Tile];
				
				if (!hilliness.NullOrEmpty() && !hilliness.Contains(settlementTile.hilliness))
					needsRelocation = true;
				if (!biomes.NullOrEmpty() && !biomes.Contains(settlementTile.PrimaryBiome))
					needsRelocation = true;

				if (!needsRelocation)
					continue;

				int targetTile = FindValidTile(settlement.Tile, hilliness, biomes, ext.searchRadius, grid);
				if (targetTile >= 0)
				{
					Find.WorldObjects.Remove(settlement);
					settlement.Tile = targetTile;
					Find.WorldObjects.Add(settlement);
				}
			}
		}
	}

	static GameComponent_SettlementMapGenerators GetOrCreateGameComponent()
	{
		GameComponent_SettlementMapGenerators comp = Current.Game.GetComponent<GameComponent_SettlementMapGenerators>();
		if (comp == null)
		{
			comp = new GameComponent_SettlementMapGenerators(Current.Game);
			Current.Game.components.Add(comp);
		}
		return comp;
	}

	static int FindValidTile(int origin, List<Hilliness> hilliness, List<BiomeDef> biomes, int radius, WorldGrid grid)
	{
		HashSet<int> visited = new HashSet<int>();
		Queue<int> queue = new Queue<int>();
		List<PlanetTile> neighbors = new List<PlanetTile>();

		visited.Add(origin);
		queue.Enqueue(origin);

		while (queue.Count > 0)
		{
			if (visited.Count > radius * radius)
				break;

			int tile = queue.Dequeue();
			grid.GetTileNeighbors(new PlanetTile(tile), neighbors);

			for (int i = 0; i < neighbors.Count; i++)
			{
				int neighborTile = neighbors[i].tileId;
				if (!visited.Add(neighborTile))
					continue;

				Tile neighborTileData = grid[neighborTile];
				bool valid = TileFinder.IsValidTileForNewSettlement(neighborTile) && !Find.WorldObjects.AnyWorldObjectAt(neighborTile);
				
				if (valid && !hilliness.NullOrEmpty() && !hilliness.Contains(neighborTileData.hilliness))
					valid = false;
				if (valid && !biomes.NullOrEmpty() && !biomes.Contains(neighborTileData.PrimaryBiome))
					valid = false;

				if (valid)
					return neighborTile;

				queue.Enqueue(neighborTile);
			}
		}
		return -1;
	}
}

[HarmonyPatch(typeof(MapGenerator), nameof(MapGenerator.GenerateMap))]
static class MapGenerator_GenerateMap_Patch
{
	static void Prefix(ref IntVec3 mapSize, MapParent parent, ref MapGeneratorDef mapGenerator, IEnumerable<GenStepWithParams> extraGenStepDefs = null, Action<Map> extraInitBeforeContentGen = null)
	{
		Settlement settlement = parent as Settlement;
		if (settlement == null)
			return;

		GameComponent_SettlementMapGenerators comp = Current.Game?.GetComponent<GameComponent_SettlementMapGenerators>();
		if (comp == null)
			return;

		MapGeneratorDef custom = comp.GetMapGenerator(settlement);
		if (custom != null)
		{
			mapGenerator = custom;
			
			IntVec3 customSize = comp.GetMapSize(settlement);
			if (customSize != IntVec3.Invalid)
				mapSize = customSize;
		}
	}
}
