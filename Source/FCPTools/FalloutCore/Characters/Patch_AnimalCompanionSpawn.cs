using HarmonyLib;
using RimWorld.Planet;

namespace FCP.Core
{
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.SpawnSetup))]
    public static class Patch_AnimalCompanionSpawn
    {
        public static void Postfix(Pawn __instance, Map map)
        {
            if (map == null)
            {
                return;
            }

            UniqueCharactersTracker tracker = UniqueCharactersTracker.Instance;
            if (tracker == null || !tracker.TryGetPawnCharacter(__instance, out UniqueCharacter character))
            {
                return;
            }

            foreach (CharacterBaseDefinition definition in character.def.definitions)
            {
                if (definition is not CharacterAnimalCompanionDefinition animalDef || animalDef.animalCharacterDef == null)
                {
                    continue;
                }

                Pawn animal = tracker.GetOrGenPawn(animalDef.animalCharacterDef);
                if (animal.Dead || animal.Spawned || animal.GetCaravan() != null)
                {
                    continue;
                }

                IntVec3 spawnCell = CellFinder.RandomClosewalkCellNear(__instance.Position, map, 3);
                GenSpawn.Spawn(animal, spawnCell, map);
                animal.SetFaction(__instance.Faction);

                if (animal.playerSettings != null)
                {
                    animal.playerSettings.Master = __instance;
                }
            }
        }
    }
}
