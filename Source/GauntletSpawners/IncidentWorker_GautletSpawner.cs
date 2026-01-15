using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using static System.Net.Mime.MediaTypeNames;

namespace GauntletSpawners
{
    public class IncidentWorker_GautletSpawner : IncidentWorker
    {
        public IncidentExtension_GautletSpawnCondition modExtension => def.GetModExtension<IncidentExtension_GautletSpawnCondition>();

        public float SpawnPoints => modExtension.spawnerPoint > 0 ? modExtension.spawnerPoint : 220f;

        public static readonly SimpleCurve PointsFactorCurve = new SimpleCurve
    {
        new CurvePoint(0f, 0.7f),
        new CurvePoint(5000f, 0.45f)
    };

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            IntVec3 cell;
            if (base.CanFireNowSub(parms) && Find.FactionManager.FirstFactionOfDef(modExtension.factionDef) != null)
            {
                return TryFindSpawnLoc(map, out cell);
            }
            return false;
        }

        public bool TryFindSpawnLoc(Map map, out IntVec3 cell)
        {
            List<IntVec3> intVec3s = new List<IntVec3>();
            IReadOnlyList<IntVec3> tempList = new List<IntVec3>(map.AllCells);
            for (int i = 0; i < tempList.Count; i++)
            {
                if (!modExtension.isAllowFogged && tempList[i].Fogged(map))
                {
                    continue;
                }
                if (!modExtension.isAllowRoofed && tempList[i].Roofed(map))
                {
                    continue;
                }                
                if (!modExtension.allowedTerrains.NullOrEmpty())
                {
                    if (modExtension.allowedTerrains.Contains(tempList[i].GetTerrain(map)))
                    {
                        intVec3s.Add(tempList[i]);
                    }
                    else
                    {
                        continue;
                    }
                }
                else if(!modExtension.disallowedTerrains.NullOrEmpty())
                {
                    if (!modExtension.disallowedTerrains.Contains(tempList[i].GetTerrain(map)))
                    {
                        intVec3s.Add(tempList[i]);
                    }
                }
                else
                {
                    intVec3s.Add(tempList[i]);
                }
            }
            if (intVec3s.Count > 0)
            {
                cell = intVec3s.RandomElement();
                return true;
            }
            cell = IntVec3.Invalid;
            return false;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            parms.points *= PointsFactorCurve.Evaluate(parms.points);
            List<Thing> thing = new List<Thing>(SpawnThing(parms, map));
            SendStandardLetter(parms, thing.RandomElement());            
            Find.TickManager.slower.SignalForceNormalSpeedShort();
            return true;
        }

        public IEnumerable<Thing> SpawnThing(IncidentParms parms,Map map)
        {
            float spawnerPoint = modExtension.spawnerPoint;
            float totalPointAvailable = parms.points;
            IntVec3 rootLoc = IntVec3.Invalid;
            if (TryFindSpawnLoc(map,out var cell))
            {
                rootLoc = cell;
            }
            if (modExtension.count <= 0)
            {
                while (totalPointAvailable > 0)
                {
                    Thing thing = ThingMaker.MakeThing(modExtension.thingDef);
                    if (thing.def.CanHaveFaction)
                    {
                        thing.SetFaction(Find.FactionManager.FirstFactionOfDef(modExtension.factionDef));
                    }
                    GenSpawn.Spawn(thing, GenRadial.RadialCellsAround(rootLoc, modExtension.radius, true).RandomElement().ClampInsideMap(map), map, WipeMode.FullRefund);
                    totalPointAvailable -= spawnerPoint;
                    yield return thing;
                }
            }
            else
            {
                for (int i = 0; i < modExtension.count; i++)
                {
                    Thing thing = ThingMaker.MakeThing(modExtension.thingDef);
                    if (thing.def.CanHaveFaction)
                    {
                        thing.SetFaction(Find.FactionManager.FirstFactionOfDef(modExtension.factionDef));
                    }
                    GenSpawn.Spawn(thing, GenRadial.RadialCellsAround(rootLoc, modExtension.radius, true).RandomElement().ClampInsideMap(map), map, WipeMode.FullRefund);
                    yield return thing;
                    Log.Message($"A{i}");
                }
            }
        }
    }
}
