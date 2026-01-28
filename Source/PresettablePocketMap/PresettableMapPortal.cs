using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI.Group;

namespace FCP.PocketMaps
{
    public class PresettableMapPortal : MapPortal
    {
        private bool isAbandoned = false;

        public bool IsAbandoned => isAbandoned;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref isAbandoned, "isAbandoned", false);
        }

        public void MarkAsAbandoned()
        {
            isAbandoned = true;
        }

        protected override Map GeneratePocketMapInt()
        {
            var ext = def.GetModExtension<ModExtensionPresettablePocketMap>();
            
            if (isAbandoned)
            {
                var message = ext?.abandonedMessage ?? "This pocket map has been abandoned and can no longer be entered.";
                Messages.Message(message, this, MessageTypeDefOf.RejectInput);
                Find.Selector.ClearSelection();
                return Map;
            }

            if (ext?.prefabDefs == null || ext.prefabDefs.Count == 0)
            {
                Log.Error($"{def.defName}: No prefabDefs specified in mod extension");
                return null;
            }

            string defName = ext.prefabDefs.RandomElement();
            var prefabDef = DefDatabase<PocketMapPrefabDef>.GetNamed(defName, false);
            if (prefabDef == null)
            {
                Log.Error($"{def.defName}: PocketMapPrefabDef '{defName}' not found");
                return null;
            }

            return GenerateFromPrefab(prefabDef);
        }

        private Map GenerateFromPrefab(PocketMapPrefabDef prefab)
        {
            var size = ParseSize(prefab.size);
            var mapGen = prefab.mapGeneratorDef ?? def.portal.pocketMapGenerator;
            if (mapGen == null)
            {
                Log.Error($"{prefab.defName}: No MapGeneratorDef specified");
                return null;
            }
            
            Map map = PocketMapUtility.GeneratePocketMap(new IntVec3(size.x, 1, size.z), mapGen, GetExtraGenSteps(), Map);
            
            ApplyFloor(map, prefab.floorDef);
            SpawnThings(map, prefab.things);
            
            var faction = prefab.factionDef != null ? Find.FactionManager.FirstFactionOfDef(prefab.factionDef) : null;
            map.info.parent.SetFaction(faction);
            
            var pawns = SpawnPawns(map, prefab.pawnKinds, faction);
            if (pawns.Count > 0 && faction != null && faction != Faction.OfPlayer)
                LordMaker.MakeNewLord(faction, new LordJob_DefendPoint(map.Center), map, pawns);

            return map;
        }

        private void ApplyFloor(Map map, TerrainDef terrain)
        {
            terrain = terrain ?? TerrainDefOf.Concrete;
            map.terrainGrid.ResetGrids();
            foreach (var cell in map.AllCells)
                map.terrainGrid.SetTerrain(cell, terrain);
        }

        private void SpawnThings(Map map, ThingsContainer container)
        {
            if (container?.items == null) return;

            foreach (var item in container.items)
            {
                if (item.thingDef == null) continue;

                if (!string.IsNullOrEmpty(item.rect))
                {
                    var r = ParseRect(item.rect);
                    for (int x = r.x1; x <= r.x2; x++)
                        for (int z = r.z1; z <= r.z2; z++)
                            TrySpawnThing(item, new IntVec3(x, 0, z), map);
                }
                else
                {
                    var pos = string.IsNullOrEmpty(item.position) ? IntVec3.Zero : ParsePosition(item.position);
                    TrySpawnThing(item, pos, map);
                }
            }
        }

        private void TrySpawnThing(PrefabItem item, IntVec3 pos, Map map)
        {
            if (!map.AllCells.Contains(pos) || Rand.Value > item.chance) return;

            var thing = ThingMaker.MakeThing(item.thingDef, item.stuff);
            if (thing is Building building)
            {
                var rot = ParseRotation(item.relativeRotation);
                
                var quality = building.TryGetComp<CompQuality>();
                if (quality != null && !string.IsNullOrEmpty(item.quality) && System.Enum.TryParse<QualityCategory>(item.quality, out var qc))
                    quality.SetQuality(qc, null);

                GenSpawn.Spawn(building, pos, map, rot);
                
                if (item.hp.HasValue && item.hp.Value > 0)
                    building.HitPoints = item.hp.Value;
            }
            else
            {
                GenSpawn.Spawn(thing, pos, map);
            }
        }

        private List<Pawn> SpawnPawns(Map map, PawnKindsContainer container, Faction faction)
        {
            var spawned = new List<Pawn>();
            if (container?.items == null) return spawned;

            foreach (var def in container.items)
            {
                if (def.pawnKindDef == null) continue;

                for (int i = 0; i < def.count; i++)
                {
                    var request = new PawnGenerationRequest(def.pawnKindDef, faction, forceGenerateNewPawn: true);
                    var pawn = PawnGenerator.GeneratePawn(request);
                    
                    if (CellFinder.TryRandomClosewalkCellNear(map.Center, map, 999, out var pos))
                    {
                        GenSpawn.Spawn(pawn, pos, map);
                        spawned.Add(pawn);
                        if (pawn.mindState != null)
                            pawn.mindState.exitMapAfterTick = -99999;
                    }
                    else
                    {
                        pawn.Destroy();
                    }
                }
            }
            
            return spawned;
        }

        private IntVec2 ParseSize(string s)
        {
            var parts = s.Trim('(', ')').Split(',');
            return parts.Length == 2 ? new IntVec2(int.Parse(parts[0]), int.Parse(parts[1])) : new IntVec2(50, 50);
        }

        private IntVec3 ParsePosition(string s)
        {
            var parts = s.Trim('(', ')').Split(',');
            return parts.Length == 3 ? new IntVec3(int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2])) : IntVec3.Zero;
        }

        private (int x1, int z1, int x2, int z2) ParseRect(string s)
        {
            var parts = s.Trim('(', ')').Split(',');
            return parts.Length == 4 ? (int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]), int.Parse(parts[3])) : (0, 0, 0, 0);
        }

        private Rot4 ParseRotation(string s)
        {
            if (string.IsNullOrEmpty(s)) return Rot4.North;
            switch (s.ToLower())
            {
                case "east":
                case "clockwise": return Rot4.East;
                case "south": return Rot4.South;
                case "west":
                case "counterclockwise": return Rot4.West;
                default: return Rot4.North;
            }
        }

        protected override IEnumerable<GenStepWithParams> GetExtraGenSteps()
        {
            yield return new GenStepWithParams(DefDatabase<GenStepDef>.GetNamed("PresettableMapEntrance"), default);
        }
    }
}
