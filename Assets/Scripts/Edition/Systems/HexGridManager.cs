using System.Collections.Generic;
using Edition.Models;
using UnityEngine;
using Controler.Editor.ViewModels;
using Domain;

namespace Edition.Systems
{
    /// Manages hex grid data: tile storage, placement, removal and lookup.
    public class HexGridManager
    {
        /// Dictionary for O(1) tile lookup by cell position.
        private readonly Dictionary<Vector3Int, GameObject> byCell = new();
        private readonly Grid grid;
        private readonly List<GameObject> tilePrefabs;
        private TerrainLayoutViewModel terrainLayoutViewModel;

        public int PrefabCount => tilePrefabs?.Count ?? 0;
        public bool HasGrid => grid != null;

        public HexGridManager(Grid grid, List<GameObject> tilePrefabs)
        {
            this.grid = grid;
            this.tilePrefabs = tilePrefabs;
        }
        
        /// <summary>
        /// Sets the terrain layout view model and subscribes to its changes
        /// </summary>
        /// <param name="viewModel"></param>
        public void SetTerrainLayout(TerrainLayoutViewModel viewModel)
        {
           terrainLayoutViewModel = viewModel;

           if (terrainLayoutViewModel != null)
           {
               LoadTilesFromTerrainLayout();
           }
        }
        
        /// <summary>
        /// Loads existing tiles from the terrain layout model into the grid
        /// </summary>
        private void LoadTilesFromTerrainLayout()
        {
            var terrainLayout = terrainLayoutViewModel.Model;
            if (terrainLayout?.Tiles == null || terrainLayout.Tiles.Count == 0) {
                return;
            }

            foreach (var tileData in terrainLayout.Tiles)
            {
                var cell = new Vector3Int(tileData.X, tileData.Y, tileData.Z);
                if (!byCell.ContainsKey(cell))
                {
                    PlaceAtCell(cell, tileData.PrefabIndex, tileData.YRotation);
                }
            }
        }

        public bool TryGetTileAt(Vector3Int cell, out GameObject tile)
        {
            return byCell.TryGetValue(cell, out tile);
        }

        public bool ContainsCell(Vector3Int cell)
        {
            return byCell.ContainsKey(cell);
        }

        public Vector3 GetCellCenterWorld(Vector3Int cell)
        {
            return grid.GetCellCenterWorld(cell);
        }

        public Vector3Int WorldToCell(Vector3 worldPos)
        {
            return grid.WorldToCell(worldPos);
        }

        
        // ReSharper disable Unity.PerformanceAnalysis
        public void PlaceAtCell(Vector3Int cell, int prefabIndex, float yRotation)
        {
            if (byCell.ContainsKey(cell)) return;
            if (tilePrefabs == null || tilePrefabs.Count == 0)
            {
                Debug.LogError("No tile prefabs assigned.");
                return;
            }

            int idx = Mathf.Clamp(prefabIndex, 0, tilePrefabs.Count - 1);
            var prefab = tilePrefabs[idx];
            Vector3 world = grid.GetCellCenterWorld(cell);

            var tile = Object.Instantiate(prefab, world, Quaternion.Euler(0f, yRotation, 0f));
            tile.name = $"{prefab.name}_{cell.x}_{cell.y}_{cell.z}";
            byCell[cell] = tile;

            // Add marker component to store tile data for serialization
            var marker = tile.AddComponent<PlacedTile>();
            marker.prefabIndex = idx;
            marker.cell = cell;
            marker.yRotation = yRotation;

            if (terrainLayoutViewModel != null)
            {
                var terrainLayout = terrainLayoutViewModel.Model as TerrainLayout;
                if (terrainLayout != null)
                {
                    var TilesData = new HexTileData
                    {
                        PrefabIndex = idx,
                        X = cell.x,
                        Y = cell.y,
                        Z = cell.z,
                        YRotation = yRotation,
                    };
                    terrainLayout.Tiles.Add(TilesData);
                }
            }
        }

        public void RemoveAtCell(Vector3Int cell)
        {
            if (byCell.TryGetValue(cell, out var obj))
            {
                Object.Destroy(obj);
                byCell.Remove(cell);

                if (terrainLayoutViewModel != null)
                {
                    var terrainLayout =  terrainLayoutViewModel.Model as TerrainLayout;
                    if (terrainLayout != null)
                    {
                        terrainLayout.Tiles.RemoveAll(t =>
                            t.X == cell.x && t.Y == cell.y && t.Z == cell.z);
                    }
                }
                return;
            }

            // Fallback: search scene markers if not in dictionary (e.g., loaded tiles)
            var markers = Object.FindObjectsByType<PlacedTile>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            foreach (var m in markers)
            {
                if (m.cell == cell)
                {
                    Object.Destroy(m.gameObject);
                    break;
                }
            }
        }

        public void UpdateTilePosition(PlacedTile tile, Vector3Int oldCell, Vector3Int newCell)
        {
            byCell.Remove(oldCell);
            byCell[newCell] = tile.gameObject;

            if (terrainLayoutViewModel != null)
            {
                var terrainLayout = terrainLayoutViewModel.Model as TerrainLayout;
                if (terrainLayout != null)
                {
                   var tileData = terrainLayout.Tiles.Find(t => 
                        t.X == oldCell.x && t.Y == oldCell.y && t.Z == oldCell.z);

                   if (tileData != null)
                   {
                       tileData.X = newCell.x;
                       tileData.Y = newCell.y;
                       tileData.Z = newCell.z;
                   }
                }
            }
        }

        public void ClearAll()
        {
            foreach (var kv in byCell)
            {
                if (kv.Value != null) Object.Destroy(kv.Value);
            }
            byCell.Clear();

            var markers = Object.FindObjectsByType<PlacedTile>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            foreach (var m in markers)
            {
                if (m != null) Object.Destroy(m.gameObject);
            }
            
            if (terrainLayoutViewModel != null)
            {
                var terrainLayout = terrainLayoutViewModel.Model as TerrainLayout;
                if (terrainLayout != null)
                {
                    terrainLayout.Tiles.Clear();
                }
            }

        }

        /// Rebuilds grid from saved map data (used when loading).
        public void RebuildFrom(MapDataDTO data)
        {
            if (data?.tiles == null) return;
            if (tilePrefabs == null || tilePrefabs.Count == 0)
            {
                Debug.LogError("No tile prefabs assigned.");
                return;
            }
            if (grid == null)
            {
                Debug.LogError("Grid is null.");
                return;
            }

            int placed = 0;
            foreach (var t in data.tiles)
            {
                int idx = Mathf.Clamp(t.prefabIndex, 0, tilePrefabs.Count - 1);
                var prefab = tilePrefabs[idx];
                var cell = new Vector3Int(t.x, t.y, t.z);
                var world = grid.GetCellCenterWorld(cell);
                var rot = Quaternion.Euler(0f, t.yRotation, 0f);

                var tile = Object.Instantiate(prefab, world, rot);
                tile.name = $"{prefab.name}_{cell.x}_{cell.y}_{cell.z}";
                byCell[cell] = tile;

                var marker = tile.AddComponent<PlacedTile>();
                marker.prefabIndex = idx;
                marker.cell = cell;
                marker.yRotation = t.yRotation;

                if (terrainLayoutViewModel != null)
                {
                    var terrainLayout = terrainLayoutViewModel.Model as TerrainLayout;
                    if (terrainLayout != null)
                    {
                        var TileData = new HexTileData
                        {
                            PrefabIndex = idx,
                            X = cell.x,
                            Y = cell.y,
                            Z = cell.z,
                            YRotation = t.yRotation,
                        };
                        terrainLayout.Tiles.Add(TileData);
                    }
                }
                placed++;
            }
            Debug.Log($"HexGridManager: Rebuilt {placed} tiles.");
        }
    }
}
