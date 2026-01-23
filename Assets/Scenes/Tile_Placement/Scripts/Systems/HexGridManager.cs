using System.Collections.Generic;
using HexGrid.Models;
using UnityEngine;

namespace HexGrid.Systems
{
    /// Manages hex grid data: tile storage, placement, removal and lookup.
    public class HexGridManager
    {
        /// Dictionary for O(1) tile lookup by cell position.
        private readonly Dictionary<Vector3Int, GameObject> _byCell = new();
        private readonly Grid _grid;
        private readonly List<GameObject> _tilePrefabs;

        public int PrefabCount => _tilePrefabs?.Count ?? 0;
        public bool HasGrid => _grid != null;

        public HexGridManager(Grid grid, List<GameObject> tilePrefabs)
        {
            _grid = grid;
            _tilePrefabs = tilePrefabs;
        }

        public bool TryGetTileAt(Vector3Int cell, out GameObject tile)
        {
            return _byCell.TryGetValue(cell, out tile);
        }

        public bool ContainsCell(Vector3Int cell)
        {
            return _byCell.ContainsKey(cell);
        }

        public Vector3 GetCellCenterWorld(Vector3Int cell)
        {
            return _grid.GetCellCenterWorld(cell);
        }

        public Vector3Int WorldToCell(Vector3 worldPos)
        {
            return _grid.WorldToCell(worldPos);
        }

        public void PlaceAtCell(Vector3Int cell, int prefabIndex, float yRotation)
        {
            if (_byCell.ContainsKey(cell)) return;
            if (_tilePrefabs == null || _tilePrefabs.Count == 0)
            {
                Debug.LogError("No tile prefabs assigned.");
                return;
            }

            int idx = Mathf.Clamp(prefabIndex, 0, _tilePrefabs.Count - 1);
            var prefab = _tilePrefabs[idx];
            Vector3 world = _grid.GetCellCenterWorld(cell);

            var tile = Object.Instantiate(prefab, world, Quaternion.Euler(0f, yRotation, 0f));
            tile.name = $"{prefab.name}_{cell.x}_{cell.y}_{cell.z}";
            _byCell[cell] = tile;

            // Add marker component to store tile data for serialization
            var marker = tile.AddComponent<PlacedTile>();
            marker.prefabIndex = idx;
            marker.cell = cell;
            marker.yRotation = yRotation;
        }

        public void RemoveAtCell(Vector3Int cell)
        {
            if (_byCell.TryGetValue(cell, out var obj))
            {
                Object.Destroy(obj);
                _byCell.Remove(cell);
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
            _byCell.Remove(oldCell);
            _byCell[newCell] = tile.gameObject;
        }

        public void ClearAll()
        {
            foreach (var kv in _byCell)
            {
                if (kv.Value != null) Object.Destroy(kv.Value);
            }
            _byCell.Clear();

            var markers = Object.FindObjectsByType<PlacedTile>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            foreach (var m in markers)
            {
                if (m != null) Object.Destroy(m.gameObject);
            }
        }

        /// Rebuilds grid from saved map data (used when loading).
        public void RebuildFrom(MapDataDTO data)
        {
            if (data?.tiles == null) return;
            if (_tilePrefabs == null || _tilePrefabs.Count == 0)
            {
                Debug.LogError("No tile prefabs assigned.");
                return;
            }
            if (_grid == null)
            {
                Debug.LogError("Grid is null.");
                return;
            }

            int placed = 0;
            foreach (var t in data.tiles)
            {
                int idx = Mathf.Clamp(t.prefabIndex, 0, _tilePrefabs.Count - 1);
                var prefab = _tilePrefabs[idx];
                var cell = new Vector3Int(t.x, t.y, t.z);
                var world = _grid.GetCellCenterWorld(cell);
                var rot = Quaternion.Euler(0f, t.yRotation, 0f);

                var tile = Object.Instantiate(prefab, world, rot);
                tile.name = $"{prefab.name}_{cell.x}_{cell.y}_{cell.z}";
                _byCell[cell] = tile;

                var marker = tile.AddComponent<PlacedTile>();
                marker.prefabIndex = idx;
                marker.cell = cell;
                marker.yRotation = t.yRotation;
                placed++;
            }
            Debug.Log($"HexGridManager: Rebuilt {placed} tiles.");
        }
    }
}
