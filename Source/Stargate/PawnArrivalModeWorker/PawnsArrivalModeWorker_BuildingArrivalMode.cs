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

            compTick.modExtension = modExtension;
            compTick.spawnRot = parms.spawnRotation;
            compTick.Spawn(pawns);
        }

        public override bool TryResolveRaidSpawnCenter(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            modExtension ??= def.GetModExtension<BuildingArrivalModeModExtension>();
            buildingSpawnDef ??= modExtension.buildingDefToSpawnFrom;
            var linkableSettings = modExtension.linkableSettings;

            IEnumerable<Building> possibleBuildingsToSpawnFrom = map.listerBuildings.AllBuildingsNonColonistOfDef(buildingSpawnDef).Union(map.listerBuildings.AllBuildingsColonistOfDef(buildingSpawnDef));
            if (!possibleBuildingsToSpawnFrom.Any())
            {
                return false; 
            }

            buildingSpawn = GenCollection.RandomElement(possibleBuildingsToSpawnFrom);
            CompAffectedByFacilities buildingSpawnFacilitiesComp = buildingSpawn.GetComp<CompAffectedByFacilities>();
            if (linkableSettings.requiresLinkable
                && buildingSpawnFacilitiesComp.LinkedFacilitiesListForReading.Any(Thing => Thing.def == linkableSettings.LinkableThingDefRequired) == false)
            {
                return false;
            }

            modExtension.tileToSpawn = ThingUtility.InteractionCell(new IntVec3(0, 0, -1), buildingSpawn.Position, buildingSpawn.Rotation);
            if (modExtension.tileToSpawn.IsValid)
            {
                return true; 
            }

            return false; 
        }
    }
}