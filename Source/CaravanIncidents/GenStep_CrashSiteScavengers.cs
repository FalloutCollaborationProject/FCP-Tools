using RimWorld;
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
    public class GenStep_CrashSiteScavengers : GenStep
    {
        public int radius;
        public ThingDef buildingDef;
        public int passengersMin;
        public int passengersMax;
        public int scavengersMin;
        public int scavengersMax;
        public List<LootTable> lootTables;
        public List<PassengerPawnkindChance> passengerPawnKinds;
        public List<PassengerPawnkindChance> scavengerPawnKinds;
        public FactionDef factionDef;
        public FactionDef scavengersFactionDef;
        public override int SeedPart => 1123123;

        private int maxTries = 30;  
        public override void Generate(Map map, GenStepParams parms)
        {

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
                HealthUtility.SimulateKilled(pawn, DamageDefOf.Crush);
                Corpse corpse = pawn.Corpse;
                corpse.Age = Find.TickManager.TicksGame + Rand.Range(1080000000, 1260000000);
                CompRottable compRottable = corpse.TryGetComp<CompRottable>();
                if (compRottable != null)
                {
                    compRottable.RotProgress += corpse.Age;
                }
                GenSpawn.Spawn(corpse, new IntVec3(Rand.Range(intVec.x - building.def.size.x - radius, intVec.x + building.def.size.x + radius), 0, Rand.Range(intVec.z - building.def.size.z - radius, intVec.z + building.def.size.z + radius)), map, WipeMode.VanishOrMoveAside);

            }
            var chanceScavengersPawnKinds = IncidentUtility.CumulativeWeights(scavengerPawnKinds);
            int scavengers = Rand.Range(scavengersMin, scavengersMax);
            Pawn[] scavs = new Pawn[scavengers];
            Faction scavFaction;
            if (scavengersFactionDef != null)
            {
                scavFaction = Find.FactionManager.FirstFactionOfDef(scavengersFactionDef);
            }
            else
            {
                Find.FactionManager.TryGetRandomNonColonyHumanlikeFaction(out scavFaction, true, false, TechLevel.Industrial, false);
            }
            building.SetFaction(scavFaction);

            for (int i = 0; i < scavengers; i++)
            {
                PawnKindDef pawnKind = PawnKindDefOf.Drifter;
                int randVal = Rand.Range(0, chanceScavengersPawnKinds.totalWeight);
                Log.Message("Random value = " + randVal);
                for (int j = 0; j < chanceScavengersPawnKinds.cumulativeWeights.Length; j++)
                {
                    if (randVal < chanceScavengersPawnKinds.cumulativeWeights[j])
                    {
                        pawnKind = scavengerPawnKinds[j].pawnKindDef;
                        break;
                    }
                }
                
                Pawn pawn = PawnGenerator.GeneratePawn(pawnKind, scavFaction);
                scavs[i] = pawn;
                GenSpawn.Spawn(pawn, new IntVec3(Rand.Range(intVec.x - building.def.size.x - radius, intVec.x + building.def.size.x + radius), 0, Rand.Range(intVec.z - building.def.size.z - radius, intVec.z + building.def.size.z + radius)), map, WipeMode.VanishOrMoveAside);

            }
            if (scavFaction.AllyOrNeutralTo(Find.FactionManager.OfPlayer))
            {
                Pawn trader = scavs.RandomElement();
                trader.mindState.wantsToTradeWithColony = true;
                PawnComponentsUtility.AddAndRemoveDynamicComponents(trader, true);
                trader.trader.traderKind = IncidentUtility.scavTrader;
                foreach (Pawn passenger in pawns)
                {
                    List<ThingWithComps> eq = new List<ThingWithComps>(passenger.equipment.AllEquipmentListForReading);
                    passenger.apparel.MoveAllToInventory();
                    foreach (ThingWithComps thing in eq)
                    {
                        passenger.equipment.TryTransferEquipmentToContainer(thing, trader.inventory.innerContainer);
                    }
                    passenger.inventory.innerContainer.TryTransferAllToContainer(trader.inventory.innerContainer);
                }
            }
            Lord lord = LordMaker.MakeNewLord(scavs[0].Faction, new LordJob_DefendPoint(intVec, 20, 16, false, false), map, scavs);


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
