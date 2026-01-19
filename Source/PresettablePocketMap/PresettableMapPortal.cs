using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace PresettablePocketMap
{
    public class PresettableMapPortal : MapPortal
    {
        protected override Map GeneratePocketMapInt()
        {
            if (!(def.GetModExtension<ModExtensionPresettablePocketMap>() is ModExtensionPresettablePocketMap me))
            {
                Log.Error("Failed to get presetDef for presettable map portal: no mod extension");
                return null;
            }
            if (me.presetDefs.Count <= 0)
            {
                Log.Error("Failed to get presetDef for presettable map portal: presetDefs list is empty");
                return null;
            }
            PocketMapPresetDef presetDef = Rand.Element(me.presetDefs.ToArray());
            IntVec3 rotatedIntVec3 = new IntVec3(presetDef.width, 1, presetDef.height);
            Map map = PocketMapUtility.GeneratePocketMap(rotatedIntVec3, def.portal.pocketMapGenerator, GetExtraGenSteps(), Map);

            //bool flipped = Rotation.AsInt % 2 == 1;
            //for (int x = 0; x < rotatedIntVec3.x; x++)
            //{
            //    for (int z = 0; z < rotatedIntVec3.z; z++)
            //    {
            //        PocketMapPresetTile cell = presetDef.rows.ElementAtOrDefault((flipped ? z : x))?.tiles.ElementAtOrDefault((flipped ? x : z));
            //        if (cell == null)
            //        {
            //            continue;
            //        }
            //        IntVec3 intVec = new IntVec3(x, 1, z);

            //        map.terrainGrid.SetTerrain(intVec, cell.foundation);
            //        map.terrainGrid.SetTerrain(intVec, cell.floor);

            //        foreach (PocketMapPresetBuilding p in cell.buildings)
            //        {
            //            Thing t = ThingMaker.MakeThing(p.buildingDef, p.stuff);
            //            if (!(t is Building b))
            //            {
            //                Log.Error("not a building");
            //                continue;
            //            }
            //            b.Rotation = p.rotation;
            //            if (p.style != null)
            //            {
            //                b.SetStyleDef(p.style);
            //            }
            //            b.compQuality?.SetQuality(p.quality, null);
            //            GenSpawn.Spawn(b, intVec, map);

            //            //if (!MapGenerator.PlayerStartSpotValid)
            //            //{
            //            //    MapGenerator.PlayerStartSpot = t.Position;
            //            //}
            //        }
            //    }
            //}

            map.terrainGrid.ResetGrids();
            foreach (IntVec3 intVec3 in map.AllCells)
            {
                if (intVec3.x == 0 || intVec3.x == map.Size.x - 1 || intVec3.z == 0 || intVec3.z == map.Size.z - 1)
                {
                    if (!(ThingMaker.MakeThing(presetDef.wallDef, presetDef.wallStuff) is Building b))
                    {
                        continue;
                    }
                    GenSpawn.Spawn(b, intVec3, map);
                }
                map.terrainGrid.SetTerrain(intVec3, TerrainDef.Named("Granite_Rough"));
                if (presetDef.floor != null)
                {
                    map.terrainGrid.SetTerrain(intVec3, presetDef.floor);
                }
            }
            foreach (PocketMapPresetRow row in presetDef.rows)
            {
                foreach (PocketMapPresetCell cell in row.cells)
                {
                    IntVec3 intVec3 = new IntVec3(cell.x - 1, 0, cell.y - 1);
                    if (!map.AllCells.Contains(intVec3))
                    {
                        string cellLog = "";
                        foreach (IntVec3 log in map.AllCells)
                        {
                            cellLog += "\n(" + log.x + ", " + log.y + ", " + log.z + ")";
                        }
                        Log.Error("cell (" + intVec3.x + ", " + intVec3.y + ", " + intVec3.z + ") not in map. all map cells:" + cellLog);
                        continue;
                    }

                    if (cell.foundation != null)
                    {
                        map.terrainGrid.SetUnderTerrain(intVec3, cell.foundation);
                    }
                    if (cell.floor != null)
                    {
                        map.terrainGrid.SetTerrain(intVec3, cell.floor);
                    }

                    foreach (PocketMapPresetBuilding p in cell.buildings)
                    {
                        Thing t = ThingMaker.MakeThing(p.buildingDef, p.stuff);
                        if (!(t is Building b))
                        {
                            Log.Error("not a building");
                            continue;
                        }
                        Rot4 rotation;
                        switch (p.rotation)
                        {
                            case "north":
                                rotation = Rot4.North;
                                break;
                            case "east":
                                rotation = Rot4.East;
                                break;
                            case "south":
                                rotation = Rot4.South;
                                break;
                            case "west":
                                rotation = Rot4.West;
                                break;
                            default:
                                Log.Error("invalid rotation, defaulting to north");
                                rotation = Rot4.North;
                                break;
                        }
                        //b.Rotation = rotation;
                        if (p.style != null)
                        {
                            b.SetStyleDef(p.style);
                        }
                        b.compQuality?.SetQuality(p.quality, null);
                        GenSpawn.Spawn(b, intVec3, map, rotation);
                    }

                    foreach (PocketMapPresetPawnKind p in cell.pawnKinds)
                    {
                        for (int i = 0; i < p.count; i++)
                        {
                            //PawnGenerationRequest req = new PawnGenerationRequest(p.pawnKindDef, p.faction, fixedBiologicalAge: p.age, fixedGender: p.gender);
                            Pawn pawn = PawnGenerator.GeneratePawn(p.pawnKindDef);
                            GenSpawn.Spawn(pawn, intVec3, map);
                        }
                    }
                }
            }

            return map;
        }

        protected override IEnumerable<GenStepWithParams> GetExtraGenSteps()
        {
            //yield return new GenStepWithParams(GenStepDefOf.AncientStockpile, default(GenStepParams));
            yield return new GenStepWithParams(DefDatabase<GenStepDef>.GetNamed("PresettableMapEntrance"), default);
        }
    }
}
