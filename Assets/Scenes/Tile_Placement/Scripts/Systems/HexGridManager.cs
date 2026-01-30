using System.Collections.Generic;
using HexGrid.Models;
using UnityEngine;

namespace HexGrid.Systems
{
    /// Manages hex grid tile dictionary: placement, removal, lookup.
    public class HexGridManager
    {
        private readonly Dictionary<Vector3Int, GameObject> _byCell = new();
        private readonly Grid _grid;
        private readonly List<GameObject> _tilePrefabs;

        /// Gets the number of available tile prefabs.
        public int PrefabCount => _tilePrefabs?.Count ?? 0;

        /// Gets whether the grid is initialized.
        public bool HasGrid => _grid != null;

        /// Initializes the grid manager with grid reference and prefabs.
        public HexGridManager(Grid grid, List<GameObject> tilePrefabs)
        {
            _grid = grid;
            _tilePrefabs = tilePrefabs;
        }

        /// Gets the tile GameObject at the specified cell.
        public bool TryGetTileAt(Vector3Int cell, out GameObject tile)
        {
            return _byCell.TryGetValue(cell, out tile);
        }

        /// Checks if a tile exists at the specified cell.
        public bool ContainsCell(Vector3Int cell)
        {
            return _byCell.ContainsKey(cell);
        }

        /// Converts cell coordinates to world position.
        public Vector3 GetCellCenterWorld(Vector3Int cell)
        {
            return _grid.GetCellCenterWorld(cell);
        }

        /// Converts world position to cell coordinates.
        public Vector3Int WorldToCell(Vector3 worldPos)
        {
            return _grid.WorldToCell(worldPos);
        }

        /// Places a tile at the specified cell with rotation.
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

            var marker = tile.AddComponent<PlacedTile>();
            marker.prefabIndex = idx;
            marker.cell = cell;
            marker.yRotation = yRotation;
        }

        /// Removes the tile at the specified cell.
        public void RemoveAtCell(Vector3Int cell)
        {
            if (_byCell.TryGetValue(cell, out var obj))
            {
                Object.Destroy(obj);
                _byCell.Remove(cell);
                return;
            }

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

        /// Updates tile position in the dictionary when moved.
        public void UpdateTilePosition(PlacedTile tile, Vector3Int oldCell, Vector3Int newCell)
        {
            _byCell.Remove(oldCell);
            _byCell[newCell] = tile.gameObject;
        }

        /// Removes all tiles from the grid.
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

        /// Rebuilds the grid from saved map data.
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
