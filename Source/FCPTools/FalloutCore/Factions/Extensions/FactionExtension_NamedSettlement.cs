using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace FCP.Factions;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class FactionExtension_NamedSettlement : DefModExtension
{
    public string settlementName;
    public List<string> settlementNames;
    public List<Hilliness> preferredHilliness = new() { Hilliness.Mountainous, Hilliness.LargeHills };
    public int searchRadius = 40;
    public int maxSettlements = 0;
}

[HarmonyPatch(typeof(World), nameof(World.FinalizeInit))]
static class NamedSettlement_FinalizeInit_Patch
{
    static void Postfix()
    {
        var grid = Find.WorldGrid;
        var factionSettlements = Find.WorldObjects.Settlements
            .ToList()
            .GroupBy(s => s.Faction)
            .ToDictionary(g => g.Key, g => g.ToList());

        foreach (var kvp in factionSettlements)
        {
            var faction = kvp.Key;
            var settlements = kvp.Value;
            var ext = faction?.def.GetModExtension<FactionExtension_NamedSettlement>();
            if (ext == null) continue;

            if (ext.maxSettlements > 0 && settlements.Count > ext.maxSettlements)
            {
                for (int i = ext.maxSettlements; i < settlements.Count; i++)
                    Find.WorldObjects.Remove(settlements[i]);
                settlements.RemoveRange(ext.maxSettlements, settlements.Count - ext.maxSettlements);
            }

            for (int i = 0; i < settlements.Count; i++)
            {
                var settlement = settlements[i];
                
                string name = null;
                if (!ext.settlementNames.NullOrEmpty() && i < ext.settlementNames.Count)
                    name = ext.settlementNames[i];
                else if (!ext.settlementName.NullOrEmpty())
                    name = ext.settlementName;
                
                if (!name.NullOrEmpty())
                    settlement.Name = name;

                if (ext.preferredHilliness.NullOrEmpty()) continue;
                if (ext.preferredHilliness.Contains(grid[settlement.Tile].hilliness)) continue;

                int targetTile = FindValidTile(settlement.Tile, ext.preferredHilliness, ext.searchRadius, grid);
                if (targetTile < 0) continue;

                Find.WorldObjects.Remove(settlement);
                settlement.Tile = targetTile;
                Find.WorldObjects.Add(settlement);
            }
        }
    }

    static int FindValidTile(int origin, List<Hilliness> hilliness, int radius, WorldGrid grid)
    {
        var visited = new HashSet<int> { origin };
        var queue = new Queue<int>();
        var neighbors = new List<PlanetTile>();
        queue.Enqueue(origin);

        while (queue.Count > 0)
        {
            if (visited.Count > radius * radius) break;

            int tile = queue.Dequeue();
            grid.GetTileNeighbors(new PlanetTile(tile), neighbors);

            foreach (var neighborTile in neighbors)
            {
                int neighbor = neighborTile.tileId;
                if (!visited.Add(neighbor)) continue;

                if (hilliness.Contains(grid[neighbor].hilliness)
                    && TileFinder.IsValidTileForNewSettlement(neighbor)
                    && !Find.WorldObjects.AnyWorldObjectAt(neighbor))
                    return neighbor;

                queue.Enqueue(neighbor);
            }
        }

        return -1;
    }
}
