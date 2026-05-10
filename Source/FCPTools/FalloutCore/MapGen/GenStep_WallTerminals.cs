using RimWorld;
using Verse;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace FCP.Core.MapGen;

public class GenStep_WallTerminals : GenStep
{
    public float spawnChance = 0.3f;
    public int minCount = 1;
    public int maxCount = 3;

    public override int SeedPart => 892347612;

    public override void Generate(Map map, GenStepParams parms)
    {
        if (!ModsConfig.IdeologyActive)
            return;

        if (Rand.Value > spawnChance)
            return;

        var oldTerminalDef = DefDatabase<ThingDef>.GetNamedSilentFail("FCP_Production_Old_Wall_Terminal");
        var cleanTerminalDef = DefDatabase<ThingDef>.GetNamedSilentFail("FCP_Production_Clean_Wall_Terminal");

        if (oldTerminalDef == null && cleanTerminalDef == null)
            return;

        List<IntVec3> potentialSpots = new List<IntVec3>();

        foreach (IntVec3 wallCell in map.AllCells)
        {
            if (!wallCell.Roofed(map) || !wallCell.InBounds(map))
                continue;

            Building edifice = wallCell.GetEdifice(map);
            if (edifice == null || !edifice.def.IsEdifice())
                continue;

            if (!edifice.def.building.isNaturalRock && edifice.def.graphicData?.linkType != LinkDrawerType.CornerFiller)
                continue;

            foreach (Rot4 rot in new[] { Rot4.North, Rot4.South, Rot4.East, Rot4.West })
            {
                IntVec3 adjacentCell = wallCell + rot.FacingCell;
                if (adjacentCell.InBounds(map) && adjacentCell.Standable(map) && adjacentCell.GetEdifice(map) == null)
                {
                    potentialSpots.Add(adjacentCell);
                }
            }
        }

        if (!potentialSpots.Any())
            return;

        int count = Rand.RangeInclusive(minCount, maxCount);
        count = Mathf.Min(count, potentialSpots.Count);

        for (int i = 0; i < count; i++)
        {
            IntVec3 spot = potentialSpots.RandomElement();
            potentialSpots.Remove(spot);

            ThingDef terminalDef;
            if (cleanTerminalDef != null && Rand.Value < 0.2f)
                terminalDef = cleanTerminalDef;
            else if (oldTerminalDef != null)
                terminalDef = oldTerminalDef;
            else
                continue;

            Rot4 rotation = Rot4.Invalid;
            foreach (Rot4 rot in new[] { Rot4.North, Rot4.South, Rot4.East, Rot4.West })
            {
                IntVec3 wallCell = spot + rot.FacingCell;
                if (!wallCell.InBounds(map))
                    continue;

                Building edifice = wallCell.GetEdifice(map);
                if (edifice != null && edifice.def.IsEdifice() &&
                    (edifice.def.building.isNaturalRock || edifice.def.graphicData?.linkType == LinkDrawerType.CornerFiller))
                {
                    rotation = rot;
                    break;
                }
            }

            if (rotation == Rot4.Invalid)
                continue;

            Thing terminal = ThingMaker.MakeThing(terminalDef);
            GenSpawn.Spawn(terminal, spot, map, rotation);
        }
    }
}
