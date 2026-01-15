global using System.Collections.Generic;
global using RimWorld;
global using Verse;
global using System.Linq;


namespace Thek_BuildingArrivalMode
{
    public class PawnsArrivalModeWorker_BuildingArrivalMode : PawnsArrivalModeWorker
    {
        private ThingDef buildingSpawnDef;
        private Building buildingSpawn;
        internal static BuildingArrivalModeModExtension modExtension;

        public override void Arrive(List<Pawn> pawns, IncidentParms parms)
        {
            modExtension = def.GetModExtension<BuildingArrivalModeModExtension>();
            buildingSpawnDef ??= modExtension.buildingDefToSpawnFrom;
            Comp_TickBuildingSpawn compTick = buildingSpawn.GetComp<Comp_TickBuildingSpawn>();

            //The arrive method links into that comp for the pawns to appear one by one, instead of all of them at once.
            compTick.modExtension = modExtension;
            compTick.spawnRot = parms.spawnRotation;
            compTick.Spawn(pawns);
        }

        public override bool TryResolveRaidSpawnCenter(IncidentParms parms)
        {
            // I tried going step by step here, making it fail if a step isn't fulfilled
            Map map = (Map)parms.target;
            modExtension ??= def.GetModExtension<BuildingArrivalModeModExtension>();
            buildingSpawnDef ??= modExtension.buildingDefToSpawnFrom;
            var linkableSettings = modExtension.linkableSettings;

            IEnumerable<Building> possibleBuildingsToSpawnFrom = map.listerBuildings.AllBuildingsNonColonistOfDef(buildingSpawnDef).Union(map.listerBuildings.AllBuildingsColonistOfDef(buildingSpawnDef));
            //Finds all the buildings defined in the modExtension in the map, and saves it up in a IEnumerable
            if (!possibleBuildingsToSpawnFrom.Any())
            {
                return false; //If it couldn't find any, it fails
            }


            buildingSpawn = GenCollection.RandomElement(possibleBuildingsToSpawnFrom);
            //Grabs a random building from the IEnumerable that has all of the buildings that we've found
            CompAffectedByFacilities buildingSpawnFacilitiesComp = buildingSpawn.GetComp<CompAffectedByFacilities>();
            if (linkableSettings.requiresLinkable
                && buildingSpawnFacilitiesComp.LinkedFacilitiesListForReading.Any(Thing => Thing.def == linkableSettings.LinkableThingDefRequired) == false)
                //If the modextensions ask for a linkable, but it can't find a linkable linked to the building, it fails
                //So in theory this should not return false if it doesnt require a linkable, or if it could find a linkable linked to the building.
            {
                return false;
            }

            modExtension.tileToSpawn = ThingUtility.InteractionCell(new IntVec3(0, 0, -1), buildingSpawn.Position, buildingSpawn.Rotation);
            //This finds the tile to spawn that should be in the same place where an interaction cell would hypothetically be drawn at
            if (modExtension.tileToSpawn.IsValid)
            {
                return true; //If the tile is valid it returns true :D
            }

            return false; //And, if it isn't, it's false
        }
    }
}