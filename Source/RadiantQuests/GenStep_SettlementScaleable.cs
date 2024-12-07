using RimWorld.BaseGen;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace FCP_RadiantQuests
{
    public class GenStep_SettlementScaleable : GenStep_Scatterer
    {
        public int settlementSizeMin = 34;
        public int settlementSizeMax = 38;
        public bool generateLoot = true;
        private bool generatePawns = true;

        private bool clearBuildingFaction;

        private static List<IntVec3> tmpCandidates = new List<IntVec3>();

        public override int SeedPart => 1806208471;

        protected override bool CanScatterAt(IntVec3 c, Map map)
        {
            if (!base.CanScatterAt(c, map))
            {
                return false;
            }
            if (!c.Standable(map))
            {
                return false;
            }
            if (c.Roofed(map))
            {
                return false;
            }
            if (!map.reachability.CanReachMapEdge(c, TraverseParms.For(TraverseMode.PassDoors)))
            {
                return false;
            }
            int min = settlementSizeMin;
            if (!new CellRect(c.x - min / 2, c.z - min / 2, min, min).FullyContainedWithin(new CellRect(0, 0, map.Size.x, map.Size.z)))
            {
                return false;
            }
            return true;
        }

        protected override void ScatterAt(IntVec3 c, Map map, GenStepParams parms, int stackCount = 1)
        {
            int randomInRange = Rand.RangeInclusive(settlementSizeMin, settlementSizeMax);
            int randomInRange2 = Rand.RangeInclusive(settlementSizeMin, settlementSizeMax);
            CellRect rect = new CellRect(c.x - randomInRange / 2, c.z - randomInRange2 / 2, randomInRange, randomInRange2);
            Faction faction = ((map.ParentFaction != null && map.ParentFaction != Faction.OfPlayer) ? map.ParentFaction : Find.FactionManager.RandomEnemyFaction());
            rect.ClipInsideMap(map);
            ResolveParams resolveParams = default(ResolveParams);
            resolveParams.rect = rect;
            resolveParams.faction = faction;
            resolveParams.settlementDontGeneratePawns = !generatePawns;
            resolveParams.settlementPawnGroupPoints = parms.sitePart.site.ActualThreatPoints;
            if(!generateLoot)
            {
                resolveParams.lootMarketValue = 0;
            }
            BaseGen.globalSettings.map = map;
            BaseGen.globalSettings.minBuildings = 1;
            BaseGen.globalSettings.minBarracks = 1;
            BaseGen.symbolStack.Push("settlement", resolveParams);
            if (faction != null && faction == Faction.OfEmpire)
            {
                BaseGen.globalSettings.minThroneRooms = 1;
                BaseGen.globalSettings.minLandingPads = 1;
            }
            BaseGen.Generate();
            if (faction != null && faction == Faction.OfEmpire && BaseGen.globalSettings.landingPadsGenerated == 0)
            {
                GenStep_Settlement.GenerateLandingPadNearby(resolveParams.rect, map, faction, out var _);
            }
            if (!clearBuildingFaction)
            {
                return;
            }
            foreach (Building item in map.listerThings.GetThingsOfType<Building>())
            {
                if (!(item is Building_Turret) && item.Faction == faction)
                {
                    item.SetFaction(null);
                }
            }
        }

    }
}
