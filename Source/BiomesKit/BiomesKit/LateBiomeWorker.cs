using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace BiomesKit;

public class LateBiomeWorker : WorldGenStep
{
	public static ModuleBase PerlinNoise;

	public bool validForPrinting = true;

	private List<BiomeDef> firstOrderBiomes = new List<BiomeDef>();

	private List<BiomeDef> secondOrderBiomes = new List<BiomeDef>();

	private List<BiomeDef> thirdOrderBiomes = new List<BiomeDef>();

	public override int SeedPart => 123456789;

    public override void GenerateFresh(string seed, PlanetLayer layer)
    {
		if (layer.IsRootSurface)
		{
            BiomesKit();
        }
    }

	private void BiomesKit()
	{
		foreach (BiomeDef item in DefDatabase<BiomeDef>.AllDefsListForReading.Where((BiomeDef x) => x.HasModExtension<BiomesKitControls>()))
		{
			switch (item.GetModExtension<BiomesKitControls>().biomePriority)
			{
			default:
				Log.Message("adding " + item.defName + "to primary biomes");
				firstOrderBiomes.Add(item);
				break;
			case 2:
				Log.Message("adding " + item.defName + "to secondary biomes");
				secondOrderBiomes.Add(item);
				break;
			case 3:
				Log.Message("adding " + item.defName + "to tertiary biomes");
				thirdOrderBiomes.Add(item);
				break;
			}
		}
		foreach (BiomeDef firstOrderBiome in firstOrderBiomes)
		{
			BiomesKitCalcs(firstOrderBiome);
		}
		foreach (BiomeDef secondOrderBiome in secondOrderBiomes)
		{
			BiomesKitCalcs(secondOrderBiome);
		}
		foreach (BiomeDef thirdOrderBiome in thirdOrderBiomes)
		{
			BiomesKitCalcs(thirdOrderBiome);
		}
	}

	private void BiomesKitCalcs(BiomeDef biomeDef)
	{
		BiomesKitControls modExtension = biomeDef.GetModExtension<BiomesKitControls>();
		float num = modExtension.minSouthLatitude * -1f;
		float num2 = modExtension.maxSouthLatitude * -1f;
		WorldGrid worldGrid = Find.WorldGrid;
		for (int i = 0; i < worldGrid.TilesCount; i++)
		{
			var tile = worldGrid[i] as SurfaceTile;
			if (tile is null) continue;
			float y = worldGrid.LongLatOf(i).y;
			int seed = Find.World.info.Seed;
			Vector3 tileCenter = worldGrid.GetTileCenter(i);
			bool flag = true;
			foreach (BiomeDef spawnOnBiome in modExtension.spawnOnBiomes)
			{
				if (tile.biome == spawnOnBiome)
				{
					flag = true;
					break;
				}
				flag = false;
			}
			if (!flag)
			{
				continue;
			}
			bool flag2 = true;
			flag2 = ((y < num && y > num2) ? true : false);
			bool flag3 = true;
			flag3 = ((y > modExtension.minNorthLatitude && y < modExtension.maxNorthLatitude) ? true : false);
			if ((!flag3 && !flag2 && modExtension.minSouthLatitude != -9999f 
				&& modExtension.minNorthLatitude != -9999f && modExtension.maxSouthLatitude != -9999f
				&& modExtension.maxNorthLatitude != 9999f) 
				|| (tile.WaterCovered && !modExtension.allowOnWater) || (!tile.WaterCovered 
				&& !modExtension.allowOnLand) || (modExtension.needRiver
				&& (tile.Rivers == null || tile.Rivers.Count == 0)))
			{
				continue;
			}
			if (modExtension.perlinCustomSeed.HasValue)
			{
				seed = modExtension.perlinCustomSeed.Value;
			}
			else if (modExtension.useAlternativePerlinSeedPreset)
			{
				seed = i;
			}
			if (modExtension.usePerlin)
			{
				PerlinNoise = new Perlin(0.1, 10.0, 0.6, 12, seed, QualityMode.Low);
				if (PerlinNoise.GetValue(tileCenter) <= modExtension.perlinCulling)
				{
					continue;
				}
			}
			if ((double)Rand.Value > Math.Pow(modExtension.frequency, 2.0) / 10000.0 || tile.elevation < modExtension.minElevation || tile.elevation > modExtension.maxElevation || tile.temperature < modExtension.minTemperature || tile.temperature > modExtension.maxTemperature || tile.rainfall < modExtension.minRainfall || tile.rainfall > modExtension.maxRainfall || (int)tile.hilliness < (int)modExtension.minHilliness || (int)tile.hilliness > (int)modExtension.maxHilliness)
			{
				continue;
			}
			var list = new List<PlanetTile>();
			worldGrid.GetTileNeighbors(i, list);
			int j = 0;
			int num3 = 0;
			bool flag4 = true;
			if (modExtension.minimumWaterNeighbors > 0)
			{
				for (int count = list.Count; j < count; j++)
				{
					if (worldGrid[list[j]].biome == BiomeDefOf.Ocean)
					{
						num3++;
						Log.Message("Water Neighbors =" + num3);
					}
					if (num3 < modExtension.minimumWaterNeighbors)
					{
						flag4 = false;
					}
					if (num3 == modExtension.minimumWaterNeighbors)
					{
						flag4 = true;
					}
				}
			}
			if (!flag4)
			{
				continue;
			}
			int num4 = 0;
			bool flag5 = true;
			if (modExtension.minimumWaterNeighbors > 0)
			{
				for (int count2 = list.Count; j < count2; j++)
				{
					if (worldGrid[list[j]].biome != BiomeDefOf.Ocean)
					{
						num4++;
					}
					if (num3 < modExtension.minimumLandNeighbors)
					{
						flag5 = false;
					}
					if (num3 == modExtension.minimumLandNeighbors)
					{
						flag5 = true;
					}
				}
			}
			if (!flag5)
			{
				continue;
			}
			if (biomeDef.workerClass.Name == "UniversalBiomeWorker")
			{
				tile.biome = biomeDef;
			}
			if (modExtension.setHills.HasValue)
			{
				_ = modExtension.setHills.Value;
				if (modExtension.spawnHills.HasValue && !modExtension.setHills.HasValue)
				{
					_ = modExtension.spawnHills.Value;
					modExtension.setHills = modExtension.spawnHills;
				}
				if (modExtension.setHills == BiomesKitHilliness.Random)
				{
					switch (Rand.Range((int)modExtension.minRandomHills.Value, (int)modExtension.maxRandomHills.Value))
					{
					case 1:
						tile.hilliness = Hilliness.Flat;
						break;
					case 2:
						tile.hilliness = Hilliness.SmallHills;
						break;
					case 3:
						tile.hilliness = Hilliness.LargeHills;
						break;
					case 4:
						tile.hilliness = Hilliness.Mountainous;
						break;
					}
				}
				else if (modExtension.setHills < BiomesKitHilliness.Random)
				{
					tile.hilliness = (Hilliness)modExtension.setHills.Value;
				}
			}
			if (modExtension.setElevation.HasValue)
			{
				tile.elevation = modExtension.setElevation.Value;
			}
		}
		Log.Message("finished biome cycle");
	}
}
