/*using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace FCP_CaravanIncidents
{
    
    public class GenStep_ActiveSkirmishSiegeRaid : GenStep
    {

        public FactionDef siegeFactionDef;
        public FactionDef raidFactionDef;

        public override int SeedPart => 1123123;

        private int maxTries = 30;  
        public override void Generate(Map map, GenStepParams parms)
        {
*//*            Log.Message(radius);
            Log.Message(buildingDef.defName);
            Log.Message(passengersMin);
            Log.Message(passengersMax);
            Log.Message(lootTables.Count);
            Log.Message(passengerPawnKinds.Count);
            Log.Message(factionDef.defName);
*//*
            Building building = (Building)ThingMaker.MakeThing(buildingDef);
            IntVec3 intVec = IntVec3.Invalid;
            for (int j = 0; j < maxTries; j++)
            {

                DropCellFinder.TryFindDropSpotNear(map.Center, map, out intVec, false, false, false, buildingDef.size, true);
                if (intVec.IsValid)
                {
                    break;
                }
            }
            if (!intVec.InBounds(map))
            {
                Log.Error("Could not find appropriate region");
                return;
            }
            GenPlace.TryPlaceThing(building, intVec, map, ThingPlaceMode.Near, rot: Rot4.East);

            //Preparing passenger generation
            var chancePawnKinds = IncidentUtility.CumulativeWeights(passengerPawnKinds);
            int passengers = Rand.Range(passengersMin, passengersMax);
            //Generating passengers
            Pawn[] pawns = new Pawn[passengers];
            LordJob_DefendPoint lordJob_DefendPoint = new LordJob_DefendPoint();

            for (int i = 0; i < passengers; i++)
            {
                PawnKindDef pawnKind = PawnKindDefOf.Drifter;
                int randVal = Rand.Range(0, chancePawnKinds.totalWeight);
                Log.Message("Random value = " + randVal);
                for (int j = 0; j < chancePawnKinds.cumulativeWeights.Length; j++)
                {
                    if(randVal < chancePawnKinds.cumulativeWeights[j])
                    {
                         pawnKind = passengerPawnKinds[j].pawnKindDef;
                        break;
                    }
                }
                Log.Message(pawnKind);
                Log.Message(factionDef);
                Pawn pawn = PawnGenerator.GeneratePawn(pawnKind, Find.FactionManager.FirstFactionOfDef(factionDef));
                pawns[i] = pawn;
                FillPassengerWithLoot(pawn);

                GenSpawn.Spawn(pawn, new IntVec3(Rand.Range(intVec.x - building.def.size.x - radius,  intVec.x + building.def.size.x + radius), 0, Rand.Range(intVec.z - building.def.size.z - radius, intVec.z + building.def.size.z +  radius)), map, WipeMode.VanishOrMoveAside);

            }
            Lord lord = LordMaker.MakeNewLord(pawns[0].Faction, new LordJob_DefendPoint(intVec, 20, 16, false, false), map, pawns);


        }

        public void FillPassengerWithLoot(Pawn pawn)
        {
            foreach(LootTable lootTable in lootTables)
            {
                if (!Rand.Chance(lootTable.chance))
                {
                    continue;
                }
                var chanceLoot = IncidentUtility.CumulativeWeights(lootTable.table);

                for (int i = 0; i < lootTable.repeat; i++)
                {
                    int randVal = Rand.Range(0, chanceLoot.totalWeight);
                    for (int j = 0; j < chanceLoot.cumulativeWeights.Length; j++)
                    {
                        if (randVal < chanceLoot.cumulativeWeights[j])
                        {
                            Thing thing = ThingMaker.MakeThing(lootTable.table[j].itemDef);
                            thing.stackCount = Rand.Range(lootTable.table[j].amountMin, lootTable.table[j].amountMax);
                            pawn.inventory.innerContainer.TryAdd(thing);
                            break;
                        }
                    }
                }

            }
        }

    }
}
*/