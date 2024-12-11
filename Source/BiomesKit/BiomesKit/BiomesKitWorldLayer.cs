using System.Collections;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace BiomesKit;

public class BiomesKitWorldLayer : WorldLayer
{
	private static readonly IntVec2 TexturesInAtlas = new IntVec2(2, 2);

	public override IEnumerable Regenerate()
	{
		foreach (object item in base.Regenerate())
		{
			yield return item;
		}
		Rand.PushState();
		Rand.Seed = Find.World.info.Seed;
		WorldGrid worldGrid = Find.WorldGrid;
		for (int i = 0; i < Find.WorldGrid.TilesCount; i++)
		{
			Tile tile = Find.WorldGrid[i];
			Vector3 tileCenter = worldGrid.GetTileCenter(i);
			if (tile.biome.HasModExtension<BiomesKitControls>())
			{
				bool flag = tile.Roads.NullOrEmpty();
				bool flag2 = tile.Rivers.NullOrEmpty();
				BiomesKitControls modExtension = tile.biome.GetModExtension<BiomesKitControls>();
				if (modExtension.uniqueHills)
				{
					if (flag2 && flag)
					{
						string text = "WorldMaterials/BiomesKit/" + tile.biome.defName + "/Hills/";
						float num = 0f;
						switch (tile.hilliness)
						{
						case Hilliness.Flat:
							text = null;
							break;
						case Hilliness.SmallHills:
							num = new FloatRange(modExtension.smallHillSizeMultiplier - 0.1f, modExtension.smallHillSizeMultiplier + 0.1f).RandomInRange;
							text = ((!(tile.temperature < modExtension.snowpilesBelow)) ? (text + "SmallHills") : (text + "SmallSnowpiles"));
							break;
						case Hilliness.LargeHills:
							num = new FloatRange(modExtension.largeHillSizeMultiplier - 0.1f, modExtension.largeHillSizeMultiplier + 0.1f).RandomInRange;
							text = ((!(tile.temperature < modExtension.snowpilesBelow)) ? (text + "LargeHills") : (text + "LargeSnowpiles"));
							break;
						case Hilliness.Mountainous:
							num = new FloatRange(modExtension.mountainSizeMultiplier - 0.1f, modExtension.mountainSizeMultiplier + 0.1f).RandomInRange;
							text += "Mountains";
							if (tile.temperature < modExtension.mountainsFullySnowyBelow)
							{
								text += "_FullySnowy";
							}
							else if (tile.temperature < modExtension.mountainsVerySnowyBelow)
							{
								text += "_VerySnowy";
							}
							else if (tile.temperature < modExtension.mountainsSnowyBelow)
							{
								text += "_Snowy";
							}
							else if (tile.temperature < modExtension.mountainsSemiSnowyBelow)
							{
								text += "_SemiSnowy";
							}
							break;
						case Hilliness.Impassable:
							num = new FloatRange(modExtension.impassableSizeMultiplier - 0.1f, modExtension.impassableSizeMultiplier + 0.1f).RandomInRange;
							text += "Impassable";
							if (tile.temperature < modExtension.impassableFullySnowyBelow)
							{
								text += "_FullySnowy";
							}
							else if (tile.temperature < modExtension.impassableVerySnowyBelow)
							{
								text += "_VerySnowy";
							}
							else if (tile.temperature < modExtension.impassableSnowyBelow)
							{
								text += "_Snowy";
							}
							else if (tile.temperature < modExtension.impassableSemiSnowyBelow)
							{
								text += "_SemiSnowy";
							}
							break;
						}
						if (text != null)
						{
							Material material = MaterialPool.MatFrom(text, ShaderDatabase.WorldOverlayTransparentLit, modExtension.materialLayer);
							LayerSubMesh subMesh = GetSubMesh(material);
							WorldRendererUtility.PrintQuadTangentialToPlanet(tileCenter, tileCenter, worldGrid.averageTileSize * num, 0.01f, subMesh, counterClockwise: false, modExtension.materialRandomRotation, printUVs: false);
							WorldRendererUtility.PrintTextureAtlasUVs(Rand.Range(0, TexturesInAtlas.x), Rand.Range(0, TexturesInAtlas.z), TexturesInAtlas.x, TexturesInAtlas.z, subMesh);
						}
					}
				}
				else if (!tile.WaterCovered && ModsConfig.IsActive("Odeum.WMBP"))
				{
					string text2 = "WorldMaterials/BiomesKit/Default/Hills/";
					switch (tile.hilliness)
					{
					case Hilliness.Flat:
						text2 = null;
						break;
					case Hilliness.SmallHills:
						text2 += "SmallHills";
						break;
					case Hilliness.LargeHills:
						text2 += "LargeHills";
						break;
					case Hilliness.Mountainous:
						text2 += "Mountains";
						break;
					case Hilliness.Impassable:
						text2 += "Impassable";
						break;
					}
					if (text2 != null)
					{
						Material material2 = MaterialPool.MatFrom(text2, ShaderDatabase.WorldOverlayTransparentLit, modExtension.materialLayer);
						LayerSubMesh subMesh2 = GetSubMesh(material2);
						WorldRendererUtility.PrintQuadTangentialToPlanet(tileCenter, tileCenter, new FloatRange(0.9f, 1.1f).RandomInRange * worldGrid.averageTileSize, 0.005f, subMesh2, counterClockwise: false, randomizeRotation: true, printUVs: false);
						WorldRendererUtility.PrintTextureAtlasUVs(Rand.Range(0, TexturesInAtlas.x), Rand.Range(0, TexturesInAtlas.z), TexturesInAtlas.x, TexturesInAtlas.z, subMesh2);
					}
				}
				if (modExtension.forested && tile.hilliness == Hilliness.Flat && flag2 && flag)
				{
					string text3 = "WorldMaterials/BiomesKit/" + tile.biome.defName + "/Forest/Forest_";
					bool flag3 = false;
					if (tile.temperature < modExtension.forestSnowyBelow)
					{
						text3 += "Snowy";
						flag3 = true;
					}
					float rainfall = tile.rainfall;
					if (rainfall < modExtension.forestSparseBelow)
					{
						text3 += "Sparse";
						flag3 = true;
					}
					else if (rainfall > modExtension.forestDenseAbove)
					{
						text3 += "Dense";
						flag3 = true;
					}
					if (!flag3)
					{
						text3 = text3.Remove(text3.Length - 1, 1);
					}
					Material material3 = MaterialPool.MatFrom(text3, ShaderDatabase.WorldOverlayTransparentLit, modExtension.materialLayer);
					LayerSubMesh subMesh3 = GetSubMesh(material3);
					WorldRendererUtility.PrintQuadTangentialToPlanet(tileCenter, tileCenter, worldGrid.averageTileSize * modExtension.materialSizeMultiplier, 0.01f, subMesh3, counterClockwise: false, modExtension.materialRandomRotation, printUVs: false);
					WorldRendererUtility.PrintTextureAtlasUVs(Rand.Range(0, TexturesInAtlas.x), Rand.Range(0, TexturesInAtlas.z), TexturesInAtlas.x, TexturesInAtlas.z, subMesh3);
				}
				if (modExtension.materialPath != "World/MapGraphics/Default")
				{
					Material material4 = MaterialPool.MatFrom(modExtension.materialPath, ShaderDatabase.WorldOverlayTransparentLit, modExtension.materialLayer);
					LayerSubMesh subMesh4 = GetSubMesh(material4);
					WorldRendererUtility.PrintQuadTangentialToPlanet(tileCenter, tileCenter, worldGrid.averageTileSize * modExtension.materialSizeMultiplier, 0.01f, subMesh4, counterClockwise: false, modExtension.materialRandomRotation, printUVs: false);
					WorldRendererUtility.PrintTextureAtlasUVs(Rand.Range(0, TexturesInAtlas.x), Rand.Range(0, TexturesInAtlas.z), TexturesInAtlas.x, TexturesInAtlas.z, subMesh4);
				}
			}
			else if (ModsConfig.IsActive("Odeum.WMBP"))
			{
				string text4 = "WorldMaterials/BiomesKit/Default/Hills/";
				switch (tile.hilliness)
				{
				case Hilliness.Flat:
					text4 = null;
					break;
				case Hilliness.SmallHills:
					text4 += "SmallHills";
					break;
				case Hilliness.LargeHills:
					text4 += "LargeHills";
					break;
				case Hilliness.Mountainous:
					text4 += "Mountains";
					break;
				case Hilliness.Impassable:
					text4 += "Impassable";
					break;
				}
				if (text4 != null)
				{
					Material material5 = MaterialPool.MatFrom(text4, ShaderDatabase.WorldOverlayTransparentLit, 3510);
					LayerSubMesh subMesh5 = GetSubMesh(material5);
					WorldRendererUtility.PrintQuadTangentialToPlanet(tileCenter, tileCenter, new FloatRange(0.9f, 1.1f).RandomInRange * worldGrid.averageTileSize, 0.005f, subMesh5, counterClockwise: false, randomizeRotation: true, printUVs: false);
					WorldRendererUtility.PrintTextureAtlasUVs(Rand.Range(0, TexturesInAtlas.x), Rand.Range(0, TexturesInAtlas.z), TexturesInAtlas.x, TexturesInAtlas.z, subMesh5);
				}
			}
		}
		Rand.PopState();
		FinalizeMesh(MeshParts.All);
	}
}
