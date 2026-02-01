using RimWorld.BaseGen;
using RimWorld.Planet;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace FCP.Core.RadiantQuests
{
    public class GenStep_PawnRescueAnimal : GenStep
    {
        private const int Size = 8;

        public override int SeedPart => 69356099;

        public override void Generate(Map map, GenStepParams parms)
        {
            Faction faction = ((map.ParentFaction != null && map.ParentFaction != Faction.OfPlayer) ? map.ParentFaction : Find.FactionManager.RandomEnemyFaction());
            CellRect rect = MapGenerator.GetVar<List<CellRect>>("UsedRects").Last();
            Pawn singlePawnToSpawn;
            foreach(Thing thing in parms.sitePart.things)
            {
                Log.Message(thing.def.defName);
            }
            singlePawnToSpawn = (Pawn)parms.sitePart.things.Take(parms.sitePart.things[0]);
            Building building =(Building)parms.sitePart.things.Take(parms.sitePart.things[0]);
            GenPlace.TryPlaceThing(building, rect.RandomCell, map, ThingPlaceMode.Near, rot: Rot4.East);
            CompAnimalCage cage = building.GetComp<CompAnimalCage>();
            cage.Refuel(100);
            cage.InsertPawn(singlePawnToSpawn);
            cage.ShouldCapture = false;
            MapGenerator.SetVar("RectOfInterest", rect);
        }
    }
}

