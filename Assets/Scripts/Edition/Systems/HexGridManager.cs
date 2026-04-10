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
        private readonly List<GameObject> tilePrefabs;
        private TerrainLayoutViewModel terrainLayoutViewModel;

        public int PrefabCount => tilePrefabs?.Count ?? 0;

        public HexGridManager(List<GameObject> tilePrefabs)
        {
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
        
        }

        public bool TryGetTileAt(Vector3Int cell, out GameObject tile)
        {
            return byCell.TryGetValue(cell, out tile);
        }

        public bool ContainsCell(Vector3Int cell)
        {
            return byCell.ContainsKey(cell);
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



            // Add marker component to store tile data for serialization

            if (terrainLayoutViewModel != null)
            {
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
                }
            }

        }

        /// Rebuilds grid from saved map data (used when loading).
        public void RebuildFrom(MapDataDTO data)
        {
           
        }
    }
}
