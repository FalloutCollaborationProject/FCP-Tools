using System.Collections;
using RimWorld.Planet;
using UnityEngine;

namespace FCP.Core
{
    public class WorldLayer_FactionTerritory : WorldLayer
    {
        private static readonly int MatTexture = Shader.PropertyToID("_MainTex");
        private static readonly int MatColor = Shader.PropertyToID("_Color");

        private readonly List<Settlement> _settlements;
        private readonly List<List<int>> _territoryGroups = [];
        private readonly HashSet<int> _processedTiles = [];
        private readonly Dictionary<int, HashSet<int>> _settlementTiles = [];
        private readonly List<WorldFeatureTextMesh_FactionLabel> _activeFactionLabels = [];
        
        private Material _territoryMat;
        private Material _territoryBorderMat;
        private Color _territoryColor;
        private Color _territoryBorderColor;
        private bool _tilesDirty = true;
        private bool _renderOverWater;
        private IntRange _territoryRadius = new(10, 50); // change later to work with whatever faction control logic we write
        
        public override bool ShouldRegenerate => _tilesDirty;
        public int CurrentTerritoryRadius => _territoryRadius.RandomInRange;
        
        public WorldLayer_FactionTerritory()
        {
            _settlements = Find.WorldObjects.Settlements;
        }
        
        private void InitMaterials()
        {
            _territoryMat = new Material(ShaderDatabase.Transparent)
            {
                renderQueue = 3560
            };
            _territoryMat.SetTexture(MatTexture, TextureCache.FactionTerritoryTex);
            _territoryMat.SetColor(MatColor, _territoryColor);
            
            _territoryBorderMat = new Material(ShaderDatabase.Transparent)
            {
                renderQueue = 3560
            };
            _territoryBorderMat.SetTexture(MatTexture, TextureCache.FactionTerritoryBorderTex);
            _territoryBorderMat.SetColor(MatColor, _territoryBorderColor);
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
            
            ClearFactionLabels();
            
            // step 1: collect settlement territories
            foreach (Settlement settlement in _settlements)
            {
                if (settlement.Faction.def.HasModExtension<ModExtension_FactionTerritory>())
                {
                    ModExtension_FactionTerritory ext =
                        settlement.Faction.def.GetModExtension<ModExtension_FactionTerritory>();

                    _territoryColor = ext.territoryColor;
                    _territoryBorderColor = ext.territoryBorderColor;
                    _renderOverWater = ext.renderTerritoryOverWater;

                    HashSet<int> tiles = TryGetTerritoryTiles(settlement.Tile);
                    _settlementTiles[settlement.Tile] = tiles;
                    
                    InitMaterials();
                }
            }
            
            // step 2: group overlapping settlements
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
                
                _territoryGroups.Add(cluster.ToList());
            }
            
            // step 3: generate a single mesh for each cluster
            foreach (List<int> territory in _territoryGroups)
            {
                TryGenerateClusterMesh(territory);
            }
            
            FinalizeMesh(MeshParts.All);
            CacheFactionLabels();
        }
        
        /// <summary>
        /// Finds all tiles within a settlement's territory radius.
        /// </summary>
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
                    
                    List<int> neighbors = [];
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
        
        /// <summary>
        /// Generates a single mesh for a cluster of overlapping territories.
        /// </summary>
        private void TryGenerateClusterMesh(List<int> tiles)
        {
            WorldGrid worldGrid = Find.WorldGrid;
            HashSet<int> tileSet = [..tiles];
            
            foreach (int tile in tiles)
            {
                if (!_processedTiles.Add(tile))
                    continue;
                
                List<int> neighbors = [];
                worldGrid.GetTileNeighbors(tile, neighbors);
                bool isBorderTile = neighbors.Any(n => !tileSet.Contains(n));
                
                Material tileMat = isBorderTile ? _territoryBorderMat : _territoryMat;
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
        
        /// <summary>
        /// Finds the center tile for a given territory.
        /// </summary>
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
        
        /// <summary>
        /// Caches the faction names to be drawn later on.
        /// </summary>
        private void CacheFactionLabels()
        {
            ClearFactionLabels();
            
            foreach (List<int> territoryGroup in _territoryGroups)
            {
                if (territoryGroup.NullOrEmpty())
                    continue;
                
                Vector3 centerWorldPos = TryGetTerritoryCenterTile(territoryGroup);
                if (centerWorldPos == Vector3.zero)
                    continue;
                
                Vector3 adjustedPos = centerWorldPos + (Vector3.down * 0.5f);
                Settlement settlement = Find.WorldObjects.SettlementAt(territoryGroup[0]);
                
                if (settlement == null)
                    continue;
                
                ModExtension_FactionTerritory ext = 
                    settlement.Faction.def.GetModExtension<ModExtension_FactionTerritory>();
                
                string factionName = settlement.Faction.def.LabelCap;
                
                WorldFeatureTextMesh_FactionLabel textMesh = new();
                textMesh.Initialize(adjustedPos, factionName, 10f, ext.factionLabelColor);
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
}