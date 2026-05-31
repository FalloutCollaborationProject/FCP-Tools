using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;

namespace FCP.Core.RadiantQuests;

public class QuestNode_GetSiteTile_BiomeFiltered : QuestNode
{
	[NoTranslate]
	public SlateRef<string> storeAs;
	public SlateRef<bool?> preferCloserTiles;
	public SlateRef<float?> minDist;
	public SlateRef<float?> maxDist;
	public List<BiomeDef> allowedBiomes;

	protected override bool TestRunInt(Slate slate)
	{
		return Find.AnyPlayerHomeMap != null;
	}

	protected override void RunInt()
	{
		Slate slate = QuestGen.slate;
		if (TryFindTile(out int tile))
		{
			slate.Set(storeAs.GetValue(slate), tile);
		}
	}

	private bool TryFindTile(out int tile)
	{
		tile = -1;
		Map map = Find.AnyPlayerHomeMap;
		if (map == null)
		{
			return false;
		}
		
		int colonyTile = map.Tile;
		int min = (int)(minDist.GetValue(QuestGen.slate) ?? 7f);
		int max = (int)(maxDist.GetValue(QuestGen.slate) ?? 27f);
		WorldGrid grid = Find.WorldGrid;
		bool prefer = preferCloserTiles.GetValue(QuestGen.slate) ?? false;
		bool filterBiomes = allowedBiomes != null && allowedBiomes.Count > 0;
		
		if (filterBiomes)
		{
			string biomeList = "";
			for (int i = 0; i < allowedBiomes.Count; i++)
			{
				if (i > 0) biomeList += ", ";
				biomeList += allowedBiomes[i].defName;
			}
			Log.Message($"[NCRCF Quest] Filtering for {allowedBiomes.Count} biomes: {biomeList}");
		}
		
		tmpCandidates.Clear();
		for (int i = 0; i < grid.TilesCount; i++)
		{
			var data = Find.WorldGrid[i];
			if (data == null)
			{
				continue;
			}
			BiomeDef biome = data.PrimaryBiome;
			if (biome == null || !biome.implemented || data.hilliness == Hilliness.Impassable)
			{
				continue;
			}
			float dist = grid.ApproxDistanceInTiles(colonyTile, i);
			if (dist < min || dist > max)
			{
				continue;
			}
			if (filterBiomes)
			{
				bool valid = false;
				for (int j = 0; j < allowedBiomes.Count; j++)
				{
					if (allowedBiomes[j] == biome)
					{
						valid = true;
						break;
					}
				}
				if (!valid)
				{
					continue;
				}
			}
			tmpCandidates.Add(i);
		}
		
		if (tmpCandidates.Count == 0)
		{
			Log.Warning("[NCRCF Quest] No valid tiles found for quest site");
			return false;
		}
		
		Log.Message($"[NCRCF Quest] Found {tmpCandidates.Count} valid candidate tiles");
		
		if (prefer)
		{
			float totalWeight = 0f;
			for (int i = 0; i < tmpCandidates.Count; i++)
			{
				float dist = grid.ApproxDistanceInTiles(colonyTile, tmpCandidates[i]);
				totalWeight += 1f / (dist + 1f);
			}
			float rand = Rand.Value * totalWeight;
			float accum = 0f;
			for (int i = 0; i < tmpCandidates.Count; i++)
			{
				float dist = grid.ApproxDistanceInTiles(colonyTile, tmpCandidates[i]);
				accum += 1f / (dist + 1f);
				if (accum >= rand)
				{
					tile = tmpCandidates[i];
					Log.Message($"[NCRCF Quest] Selected tile {tile} with biome {Find.WorldGrid[tile].PrimaryBiome.defName}");
					return true;
				}
			}
		}
		
		tile = tmpCandidates.RandomElement();
		Log.Message($"[NCRCF Quest] Selected random tile {tile} with biome {Find.WorldGrid[tile].PrimaryBiome.defName}");
		return true;
	}
	
	private static List<int> tmpCandidates = new List<int>();
}
