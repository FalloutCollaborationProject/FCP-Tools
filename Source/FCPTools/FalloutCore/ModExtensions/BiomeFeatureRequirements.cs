using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace FCP.Core;

// Credits to SirMashedPotato for this code from the ESCP.

[UsedImplicitly]
public class BiomeFeatureRequirements : DefModExtension
{
    public bool requireRiver = false;
    public bool requireCoast = false;
    public bool requireCaves = false;
    public bool requireHills = false;
}

[HarmonyPatch]
public static class BiomeFeatureRequirementsPatches
{
    
    /// <summary>
    /// Limit certain trolls to tiles with specific landmarks
    /// </summary>
    [HarmonyPatch(typeof(WildAnimalSpawner))]
    [HarmonyPatch("CommonalityOfAnimalNow")]
    public static class WildAnimalSpawner_CommonalityOfAnimalNow_Patch
    {
        [HarmonyPostfix]
        public static void TrollFeatureRequirementsPatch(PawnKindDef def, ref Map ___map, ref float __result)
        {
            var extension = def.race.GetModExtension<BiomeFeatureRequirements>();

            if (extension is null) 
                return;
            
            if (extension.requireCaves && !Find.World.HasCaves(___map.Tile))
            {
                __result = 0;
                return;
            }
            if (extension.requireCoast && !Find.World.CoastDirectionAt(___map.Tile).IsValid)
            {
                __result = 0;
                return;
            }
            if (extension.requireHills && Find.WorldGrid[___map.Tile].hilliness == Hilliness.Flat)
            {
                __result = 0;
                return;
            }
            if (extension.requireRiver && Find.WorldGrid[___map.Tile].Rivers == null)
            {
                __result = 0;
                return;
            }
        }
    }
}