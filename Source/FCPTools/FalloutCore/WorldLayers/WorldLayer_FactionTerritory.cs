using System.Collections;
using FCP.Factions;
using RimWorld.Planet;
using UnityEngine;

namespace FCP.Core;

public class WorldLayer_FactionTerritory : WorldDrawLayer
{
    private static readonly int MatTexture = Shader.PropertyToID("_MainTex");
    private static readonly int MatColor = Shader.PropertyToID("_Color");

    private readonly List<Settlement> _settlements;
    private readonly List<(int settlementTile, List<int> tiles)> _territoryGroups = [];
    private readonly HashSet<int> _processedTiles = [];
    private readonly Dictionary<int, HashSet<int>> _settlementTiles = [];
    private readonly Dictionary<int, Material> _territoryMats = [];
    private readonly Dictionary<int, Material> _territoryBorderMats = [];
    private readonly List<WorldFeatureTextMesh_FactionLabel> _activeFactionLabels = [];

    private bool _tilesDirty = true;
    private bool _renderOverWater;
    private IntRange _territoryRadius = new(10, 50);

    public override bool ShouldRegenerate => _tilesDirty;
    public int CurrentTerritoryRadius => _territoryRadius.RandomInRange;

    public WorldLayer_FactionTerritory()
    {
        _settlements = Find.WorldObjects.Settlements;
    }

    private void InitMaterials(int settlementTile, Color territoryColor, Color territoryBorderColor)
    {
        Material territoryMat = new Material(ShaderDatabase.Transparent)
        {
            renderQueue = 3560
        };
        territoryMat.SetTexture(MatTexture, TextureCache.FactionTerritoryTex);
        territoryMat.SetColor(MatColor, territoryColor);
        _territoryMats[settlementTile] = territoryMat;

        Material territoryBorderMat = new Material(ShaderDatabase.Transparent)
        {
            renderQueue = 3560
        };
        territoryBorderMat.SetTexture(MatTexture, TextureCache.FactionTerritoryBorderTex);
        territoryBorderMat.SetColor(MatColor, territoryBorderColor);
        _territoryBorderMats[settlementTile] = territoryBorderMat;
    }

    private void ClearMaterials()
    {
        foreach (Material mat in _territoryMats.Values)
            UnityEngine.Object.Destroy(mat);
        _territoryMats.Clear();

        foreach (Material mat in _territoryBorderMats.Values)
            UnityEngine.Object.Destroy(mat);
        _territoryBorderMats.Clear();
    }

    public override IEnumerable Regenerate()
    {
        if (!_tilesDirty)
            yield break;

        _tilesDirty = false;
        SetDirty();

        subMeshes.Clear();
        _processedTiles.Clear();
        _settlementTiles.Clear();
        _territoryGroups.Clear();
        ClearMaterials();

        ClearFactionLabels();

        foreach (Settlement settlement in _settlements)
        {
            if (settlement.Faction.def.HasModExtension<FactionExtension_HasTerritory>())
            {
                FactionExtension_HasTerritory ext =
                    settlement.Faction.def.GetModExtension<FactionExtension_HasTerritory>();

                _renderOverWater = ext.renderTerritoryOverWater;

                HashSet<int> tiles = TryGetTerritoryTiles(settlement.Tile);
                _settlementTiles[settlement.Tile] = tiles;

                InitMaterials(settlement.Tile, ext.territoryColor, ext.territoryBorderColor);
            }
        }

        HashSet<int> visited = [];
        foreach (var kvp in _settlementTiles)
        {
            if (visited.Contains(kvp.Key))
                continue;

            HashSet<int> cluster = [];
            Queue<int> queue = new();
            queue.Enqueue(kvp.Key);
            visited.Add(kvp.Key);

            while (queue.Count > 0)
            {
                int tile = queue.Dequeue();
                cluster.UnionWith(_settlementTiles[tile]);

                foreach (int other in _settlementTiles.Keys)
                {
                    if (visited.Contains(other) ||
                        !_settlementTiles[other].Overlaps(_settlementTiles[tile]))
                        continue;

                    queue.Enqueue(other);
                    visited.Add(other);
                }
            }

            _territoryGroups.Add((kvp.Key, cluster.ToList()));
        }

        foreach (var (settlementTile, territory) in _territoryGroups)
        {
            TryGenerateClusterMesh(settlementTile, territory);
        }

        FinalizeMesh(MeshParts.All);
        CacheFactionLabels();
    }

    private HashSet<int> TryGetTerritoryTiles(int centerTile)
    {
        WorldGrid worldGrid = Find.WorldGrid;
        HashSet<int> territory = [];
        Queue<int> openSet = new();
        HashSet<int> visited = [];

        openSet.Enqueue(centerTile);
        visited.Add(centerTile);

        int currentRadius = 0;
        while (openSet.Count > 0 && currentRadius < CurrentTerritoryRadius)
        {
            int layerSize = openSet.Count;
            for (int i = 0; i < layerSize; i++)
            {
                int tile = openSet.Dequeue();

                if (worldGrid[tile].WaterCovered && !_renderOverWater)
                    continue;

                territory.Add(tile);

                List<PlanetTile> neighbors = [];
                worldGrid.GetTileNeighbors(tile, neighbors);
                foreach (int neighbor in neighbors)
                {
                    if (visited.Add(neighbor))
                    {
                        openSet.Enqueue(neighbor);
                    }
                }
            }
            currentRadius++;
        }
        return territory;
    }

    private void TryGenerateClusterMesh(int settlementTile, List<int> tiles)
    {
        if (!_territoryMats.TryGetValue(settlementTile, out Material territoryMat) ||
            !_territoryBorderMats.TryGetValue(settlementTile, out Material territoryBorderMat))
            return;

        WorldGrid worldGrid = Find.WorldGrid;
        HashSet<int> tileSet = [..tiles];

        foreach (int tile in tiles)
        {
            if (!_processedTiles.Add(tile))
                continue;

            List<PlanetTile> neighbors = [];
            worldGrid.GetTileNeighbors(tile, neighbors);
            bool isBorderTile = neighbors.Any(n => !tileSet.Contains(n));

            Material tileMat = isBorderTile ? territoryBorderMat : territoryMat;
            LayerSubMesh subMesh = GetSubMesh(tileMat);
            List<Vector3> verts = [];
            worldGrid.GetTileVertices(tile, verts);

            int count = subMesh.verts.Count;
            int vertCount = verts.Count;

            for (int i = 0; i < vertCount; i++)
            {
                subMesh.verts.Add(verts[i] + verts[i].normalized);
                subMesh.uvs.Add((
                    GenGeo.RegularPolygonVertexPosition(vertCount, i)
                    + Vector2.one) / 2f);

                if (i >= vertCount - 2)
                    continue;

                subMesh.tris.Add(count + i + 2);
                subMesh.tris.Add(count + i + 1);
                subMesh.tris.Add(count);
            }
        }
    }

    private static Vector3 TryGetTerritoryCenterTile(List<int> tiles)
    {
        WorldGrid worldGrid = Find.WorldGrid;
        Vector3 sum = Vector3.zero;

        foreach (int tile in tiles)
        {
            sum += worldGrid.GetTileCenter(tile);
        }
        return tiles.Count > 0 ? sum / tiles.Count : Vector3.zero;
    }

    private void CacheFactionLabels()
    {
        ClearFactionLabels();

        foreach (var (settlementTile, territoryGroup) in _territoryGroups)
        {
            if (territoryGroup.NullOrEmpty())
                continue;

            Vector3 centerWorldPos = TryGetTerritoryCenterTile(territoryGroup);
            if (centerWorldPos == Vector3.zero)
                continue;

            Vector3 adjustedPos = centerWorldPos + (Vector3.down * 0.5f);
            Settlement settlement = Find.WorldObjects.SettlementAt(settlementTile);

            if (settlement == null)
                continue;

            FactionExtension_HasTerritory ext =
                settlement.Faction.def.GetModExtension<FactionExtension_HasTerritory>();

            string factionName = settlement.Faction.def.LabelCap;

            WorldFeatureTextMesh_FactionLabel textMesh = new();
            textMesh.Initialize(adjustedPos, factionName, 10f, ext.factionLabelColor, this.planetLayer);
            textMesh.SetActive(true);
            _activeFactionLabels.Add(textMesh);
        }
    }

    private void ClearFactionLabels()
    {
        foreach (WorldFeatureTextMesh_FactionLabel label in _activeFactionLabels)
        {
            label.SetActive(false);
            label.Destroy();
        }
        _activeFactionLabels.Clear();
    }

    public void MarkDirty()
    {
        _tilesDirty = true;
        ClearFactionLabels();
    }
}
