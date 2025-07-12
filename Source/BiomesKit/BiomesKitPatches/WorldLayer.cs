using System.Linq;
using BiomesKit;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;	

namespace BiomesKitPatches;

[HarmonyPatch(typeof(WorldDrawLayer_Hills), "Regenerate")]
internal static class WorldLayer
{
	internal static void Prefix()
	{
		foreach (BiomeDef item in DefDatabase<BiomeDef>.AllDefsListForReading.Where((BiomeDef x) => x.HasModExtension<BiomesKitControls>()))
		{
			_ = item;
			if (ModsConfig.IsActive("Odeum.WMBP"))	
			{
				Material value = MaterialPool.MatFrom("Transparent", ShaderDatabase.WorldOverlayTransparentLit, 3510);
				AccessTools.Field(typeof(WorldMaterials), "SmallHills").SetValue(null, value);
				AccessTools.Field(typeof(WorldMaterials), "LargeHills").SetValue(null, value);
				AccessTools.Field(typeof(WorldMaterials), "Mountains").SetValue(null, value);
				AccessTools.Field(typeof(WorldMaterials), "ImpassableMountains").SetValue(null, value);
				break;
			}
		}
	}
}
