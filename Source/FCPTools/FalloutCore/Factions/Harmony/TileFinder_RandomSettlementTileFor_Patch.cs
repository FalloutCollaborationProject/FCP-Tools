using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

namespace FCP.Factions;

[HarmonyPatch(typeof(TileFinder), nameof(TileFinder.RandomSettlementTileFor), new[] { typeof(Faction), typeof(bool), typeof(Predicate<PlanetTile>) })]
public static class TileFinder_RandomSettlementTileFor_Patch
{
	[ThreadStatic]
	internal static bool Active;

	public static bool Prefix(Faction faction, bool mustBeAutoChoosable, ref PlanetTile __result)
	{
		if (Active)
			return true;

		if (!SpawnConfigHelper.TryBuild(faction, out var ext, out var hasBiomeFilter, out var anchors))
			return true;

		Active = true;
		try
		{
			Predicate<PlanetTile> predicate = SpawnConfigHelper.MakePredicate(ext, hasBiomeFilter, anchors);
			__result = SpawnConfigHelper.AnyValidTile(Find.WorldGrid.Surface, predicate)
				? TileFinder.RandomSettlementTileFor(faction, mustBeAutoChoosable, predicate)
				: TileFinder.RandomSettlementTileFor(faction, mustBeAutoChoosable);
		}
		finally
		{
			Active = false;
		}
		return false;
	}
}

[HarmonyPatch(typeof(TileFinder), nameof(TileFinder.RandomSettlementTileFor), new[] { typeof(PlanetLayer), typeof(Faction), typeof(bool), typeof(Predicate<PlanetTile>) })]
public static class TileFinder_RandomSettlementTileFor_LayerPatch
{
	public static bool Prefix(PlanetLayer layer, Faction faction, bool mustBeAutoChoosable, ref PlanetTile __result)
	{
		if (TileFinder_RandomSettlementTileFor_Patch.Active)
			return true;

		if (!SpawnConfigHelper.TryBuild(faction, out var ext, out var hasBiomeFilter, out var anchors))
			return true;

		TileFinder_RandomSettlementTileFor_Patch.Active = true;
		try
		{
			Predicate<PlanetTile> predicate = SpawnConfigHelper.MakePredicate(ext, hasBiomeFilter, anchors);
			__result = SpawnConfigHelper.AnyValidTile(layer, predicate)
				? TileFinder.RandomSettlementTileFor(layer, faction, mustBeAutoChoosable, predicate)
				: TileFinder.RandomSettlementTileFor(layer, faction, mustBeAutoChoosable);
		}
		finally
		{
			TileFinder_RandomSettlementTileFor_Patch.Active = false;
		}
		return false;
	}
}

internal static class SpawnConfigHelper
{
	internal static bool TryBuild(Faction faction, out FactionExtension_SettlementControl ext, out bool hasBiomeFilter, out List<PlanetTile> anchors)
	{
		ext = faction?.def.GetModExtension<FactionExtension_SettlementControl>();
		hasBiomeFilter = false;
		anchors = null;

		if (ext == null)
			return false;

		hasBiomeFilter = ext.allowedBiomes != null && ext.allowedBiomes.Count > 0;

		if (ext.clustered)
		{
			var bases = Find.WorldObjects.SettlementBases;
			for (int i = 0; i < bases.Count; i++)
			{
				if (bases[i].Faction == faction)
				{
					anchors ??= new List<PlanetTile>();
					anchors.Add(bases[i].Tile);
				}
			}
		}

		return hasBiomeFilter || anchors != null;
	}

	internal static Predicate<PlanetTile> MakePredicate(FactionExtension_SettlementControl ext, bool hasBiomeFilter, List<PlanetTile> anchors)
	{
		return tile =>
		{
			if (hasBiomeFilter)
			{
				var data = Find.WorldGrid[(int)tile];
				if (data == null || !ext.allowedBiomes.Contains(data.PrimaryBiome))
					return false;
			}
			if (anchors != null)
			{
				for (int i = 0; i < anchors.Count; i++)
				{
					if (Find.WorldGrid.TraversalDistanceBetween(anchors[i], tile, true, ext.clusterRadius + 1) <= ext.clusterRadius)
						return true;
				}
				return false;
			}
			return true;
		};
	}

	// Cheap existence check so we never invoke vanilla's real search (and its internal error log)
	// against a predicate that can't possibly be satisfied anywhere on the map.
	internal static bool AnyValidTile(PlanetLayer layer, Predicate<PlanetTile> predicate)
	{
		for (int i = 0; i < layer.TilesCount; i++)
		{
			Tile tile = layer[i];
			if (!tile.PrimaryBiome.canBuildBase || !tile.PrimaryBiome.implemented || tile.hilliness == Hilliness.Impassable)
				continue;
			if (predicate(tile.tile))
				return true;
		}
		return false;
	}
}
