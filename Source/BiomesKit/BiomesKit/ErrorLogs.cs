using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace BiomesKit;

[StaticConstructorOnStartup]
public static class ErrorLogs
{
	static ErrorLogs()
	{
		foreach (BiomeDef item in DefDatabase<BiomeDef>.AllDefsListForReading.Where((BiomeDef x) => x.HasModExtension<BiomesKitControls>()))
		{
			BiomesKitControls modExtension = item.GetModExtension<BiomesKitControls>();
			HashSet<BiomeDef> hashSet = new HashSet<BiomeDef>();
			foreach (BiomeDef spawnOnBiome in modExtension.spawnOnBiomes)
			{
				if (!hashSet.Add(spawnOnBiome))
				{
					Log.Warning("[BiomesKit] XML Config Error: " + item?.ToString() + ": spawnOnBiomes includes " + spawnOnBiome?.ToString() + " twice.");
				}
			}
			if (modExtension.materialPath != "World/MapGraphics/Default")
			{
				MaterialPool.MatFrom(modExtension.materialPath, ShaderDatabase.WorldOverlayTransparentLit, modExtension.materialLayer);
			}
			if (modExtension.materialLayer >= 3560)
			{
				Log.Warning("[BiomesKit] XML Config Error: " + item?.ToString() + ": The materialLayer is set to 3560 or higher, making the material display on top of the selection indicator.");
			}
			if (!modExtension.allowOnLand && !modExtension.allowOnWater)
			{
				Log.Warning("[BiomesKit] XML Config Error: " + item?.ToString() + ": Biome is disallowed on both land and water and will never spawn.");
			}
			if (modExtension.minTemperature > modExtension.maxTemperature)
			{
				Log.Warning("[BiomesKit] XML Config Error: " + item?.ToString() + ": minTemperature set above maxTemperature.");
			}
			if (modExtension.minNorthLatitude > modExtension.maxNorthLatitude)
			{
				Log.Warning("[BiomesKit] XML Config Error: " + item?.ToString() + ": minNorthLatitude set above maxNorthLatitude.");
			}
			if (modExtension.minSouthLatitude > modExtension.maxSouthLatitude)
			{
				Log.Warning("[BiomesKit] XML Config Error: " + item?.ToString() + ": minSouthLatitude set above maxSouthLatitude.");
			}
			if ((int)modExtension.minHilliness > (int)modExtension.maxHilliness)
			{
				Log.Warning("[BiomesKit] XML Config Error: " + item?.ToString() + ": minHilliness set above maxHilliness.");
			}
			if (modExtension.minElevation > modExtension.maxElevation)
			{
				Log.Warning("[BiomesKit] XML Config Error: " + item?.ToString() + ": minElevation set above maxElevation.");
			}
			if (modExtension.minRainfall > modExtension.maxRainfall)
			{
				Log.Warning("[BiomesKit] XML Config Error: " + item?.ToString() + ": minRainfall set above maxRainfall.");
			}
			if (modExtension.frequency > 100)
			{
				Log.Warning("[BiomesKit] XML Config Error: " + item?.ToString() + ": frequency set above 100. Frequency accepts values 1-100. Setting Frequency higher than that is not supported.");
			}
			if (!modExtension.usePerlin && modExtension.useAlternativePerlinSeedPreset)
			{
				Log.Warning("[BiomesKit] XML Config Error: " + item?.ToString() + ": usePerlin is false but useAlternativePerlinSeedPreset is true. useAlternativePerlinSeedPreset should be false if usePerlin is set to false.");
			}
			if (!modExtension.usePerlin && modExtension.perlinCustomSeed.HasValue)
			{
				Log.Warning("[BiomesKit] XML Config Error: " + item?.ToString() + ": usePerlin is false but perlinCustomSeed is assigned. perlinCustomSeed will not be read if usePerlin is set to false.");
			}
		}
	}
}
