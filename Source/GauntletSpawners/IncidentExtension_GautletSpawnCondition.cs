using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace GauntletSpawners
{
    public class IncidentExtension_GautletSpawnCondition : DefModExtension
    {
        public List<TerrainDef> disallowedTerrains;

        public List<TerrainDef> allowedTerrains;

        public bool isAllowRoofed;

        public bool isAllowFogged;

        public float radius = 1f;

        public FactionDef factionDef;

        public ThingDef thingDef;

        public int spawnerPoint;

        public int count;
    }
}
