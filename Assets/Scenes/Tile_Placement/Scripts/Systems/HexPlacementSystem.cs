using System.Collections.Generic;
using HexGrid.Models;
using UnityEngine;
using UnityEngine.InputSystem;

namespace HexGrid.Systems
{
    /// Core placement logic: raycast, place/remove, preview, dictionary + markers.
    public class HexPlacementSystem : MonoBehaviour
    {
        [Header("Grid & Prefabs")]
        [SerializeField] private Grid grid;
        [SerializeField] private List<GameObject> tilePrefabs = new();
        [SerializeField] private int currentPrefabIndex = 0;

        [Header("Placement")]
        [SerializeField] private LayerMask groundMask;
        [SerializeField] private GameObject preview;
        [SerializeField] private bool allowDeleteWithRightClick = true;

        // External blocker (UI hover etc.)
        public System.Func<bool> ShouldBlockInput;

        private readonly Dictionary<Vector3Int, GameObject> _byCell = new();

        public int PrefabCount => tilePrefabs?.Count ?? 0;
        public bool HasGrid => grid != null;

        private void Update()
        {
            if (ShouldBlockInput != null && ShouldBlockInput()) return;

            Vector2 mouse = Mouse.current.position.ReadValue();
            Ray ray = Camera.main.ScreenPointToRay(mouse);
            if (!Physics.Raycast(ray, out var hit, 1000f, ~0)) return;

            if ((groundMask & (1 << hit.collider.gameObject.layer)) == 0) return;
            if (grid == null) { Debug.LogError("HexPlacementSystem: Grid is null."); return; }

            Vector3Int cell = grid.WorldToCell(hit.point);
            Vector3 world = grid.GetCellCenterWorld(cell);

            if (preview != null) preview.transform.position = world;

            if (Mouse.current.leftButton.wasPressedThisFrame) PlaceAtCell(cell);
            if (allowDeleteWithRightClick && Mouse.current.rightButton.wasPressedThisFrame) RemoveAtCell(cell);
        }

        public void PlaceAtCell(Vector3Int cell)
        {
            if (_byCell.ContainsKey(cell)) return;
            if (tilePrefabs == null || tilePrefabs.Count == 0) { Debug.LogError("No tile prefabs assigned."); return; }

            int idx = Mathf.Clamp(currentPrefabIndex, 0, tilePrefabs.Count - 1);
            var prefab = tilePrefabs[idx];
            Vector3 world = grid.GetCellCenterWorld(cell);
            float yRot = preview != null ? preview.transform.eulerAngles.y : 0f;

            var tile = Instantiate(prefab, world, Quaternion.Euler(0f, yRot, 0f));
            tile.name = $"{prefab.name}_{cell.x}_{cell.y}_{cell.z}";
            _byCell[cell] = tile;

            var marker = tile.AddComponent<PlacedTile>();
            marker.prefabIndex = idx;
            marker.cell = cell;
            marker.yRotation = yRot;
        }

        public void RemoveAtCell(Vector3Int cell)
        {
            if (_byCell.TryGetValue(cell, out var obj))
            {
                Destroy(obj);
                _byCell.Remove(cell);
                return;
            }

            // Fallback: look up by marker
            var markers = Object.FindObjectsByType<PlacedTile>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            foreach (var m in markers)
            {
                if (m.cell == cell)
                {
                    Destroy(m.gameObject);
                    break;
                }
            }
        }

        public void ClearAll()
        {
            foreach (var kv in _byCell) if (kv.Value != null) Destroy(kv.Value);
            _byCell.Clear();

            var markers = Object.FindObjectsByType<PlacedTile>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            foreach (var m in markers) if (m != null) Destroy(m.gameObject);
        }

        public void RebuildFrom(MapDataDTO data)
        {
            if (data?.tiles == null) return;
            if (tilePrefabs == null || tilePrefabs.Count == 0) { Debug.LogError("No tile prefabs assigned."); return; }
            if (grid == null) { Debug.LogError("Grid is null."); return; }

            int placed = 0;
            foreach (var t in data.tiles)
            {
                int idx = Mathf.Clamp(t.prefabIndex, 0, tilePrefabs.Count - 1);
                var prefab = tilePrefabs[idx];
                var cell = new Vector3Int(t.x, t.y, t.z);
                var world = grid.GetCellCenterWorld(cell);
                var rot = Quaternion.Euler(0f, t.yRotation, 0f);

                var tile = Instantiate(prefab, world, rot);
                tile.name = $"{prefab.name}_{cell.x}_{cell.y}_{cell.z}";
                _byCell[cell] = tile;

                var marker = tile.AddComponent<PlacedTile>();
                marker.prefabIndex = idx;
                marker.cell = cell;
                marker.yRotation = t.yRotation;
                placed++;
            }
            Debug.Log($"HexPlacementSystem: Rebuilt {placed} tiles.");
        }
    }
}
