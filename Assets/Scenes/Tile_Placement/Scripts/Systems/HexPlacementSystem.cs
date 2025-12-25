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

        [Header("Selection")]
        [SerializeField] private Material highlightMaterial;

        // External blocker (UI hover etc.)
        public System.Func<bool> ShouldBlockInput;

        private readonly Dictionary<Vector3Int, GameObject> _byCell = new();
        private readonly HashSet<PlacedTile> selectedTiles = new();
        private readonly Dictionary<PlacedTile, Material> originalMaterials = new();
        private Material previewOriginalMaterial;
        private readonly List<GameObject> additionalPreviews = new();
        private PlacedTile referenceTile;
        private bool hidePreviewsUntilMouseMove = false;
        private Vector2 lastMousePosition;

        [Header("Keyboard Movement")]
        [SerializeField] private float initialKeyDelay = 0.3f;
        [SerializeField] private float keyRepeatInterval = 0.1f;
        private float keyRepeatTimer = 0f;
        private Vector3Int? lastKeyDirection = null;

        public int PrefabCount => tilePrefabs?.Count ?? 0;
        public bool HasGrid => grid != null;

        private void Update()
        {
            if (ShouldBlockInput != null && ShouldBlockInput()) return;

            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                DeselectAllTiles();
                return;
            }

            if (selectedTiles.Count > 0)
            {
                Vector3Int? currentDirection = null;
                if (Keyboard.current.upArrowKey.isPressed) currentDirection = new Vector3Int(0, 1, 0);
                else if (Keyboard.current.downArrowKey.isPressed) currentDirection = new Vector3Int(0, -1, 0);
                else if (Keyboard.current.rightArrowKey.isPressed) currentDirection = new Vector3Int(1, 0, 0);
                else if (Keyboard.current.leftArrowKey.isPressed) currentDirection = new Vector3Int(-1, 0, 0);

                if (currentDirection.HasValue)
                {
                    if (currentDirection != lastKeyDirection)
                    {
                        MoveSelectedTilesInDirection(currentDirection.Value);
                        lastKeyDirection = currentDirection;
                        keyRepeatTimer = initialKeyDelay;
                    }
                    else
                    {
                        keyRepeatTimer -= Time.deltaTime;
                        if (keyRepeatTimer <= 0f)
                        {
                            MoveSelectedTilesInDirection(currentDirection.Value);
                            keyRepeatTimer = keyRepeatInterval;
                        }
                    }
                    return;
                }
                else
                {
                    lastKeyDirection = null;
                    keyRepeatTimer = 0f;
                }
            }
            else
            {
                lastKeyDirection = null;
                keyRepeatTimer = 0f;
            }

            Vector2 mouse = Mouse.current.position.ReadValue();

            if (hidePreviewsUntilMouseMove && mouse != lastMousePosition)
            {
                hidePreviewsUntilMouseMove = false;
            }
            lastMousePosition = mouse;

            Ray ray = Camera.main.ScreenPointToRay(mouse);
            if (!Physics.Raycast(ray, out var hit, 1000f, ~0)) return;

            if ((groundMask & (1 << hit.collider.gameObject.layer)) == 0) return;
            if (grid == null) { Debug.LogError("HexPlacementSystem: Grid is null."); return; }

            Vector3Int cell = grid.WorldToCell(hit.point);
            Vector3 world = grid.GetCellCenterWorld(cell);

            if (preview != null) preview.transform.position = world;

            bool canPlaceAll = CanPlaceAllSelectedTilesAt(cell);

            if (!hidePreviewsUntilMouseMove)
            {
                SetPreviewsVisibility(canPlaceAll);
                UpdateAdditionalPreviews(cell);
            }

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                bool ctrlHeld = Keyboard.current.leftCtrlKey.isPressed || Keyboard.current.rightCtrlKey.isPressed;

                if (_byCell.TryGetValue(cell, out GameObject tileObj))
                {
                    PlacedTile clickedTile = tileObj.GetComponent<PlacedTile>();
                    if (clickedTile != null)
                    {
                        if (ctrlHeld)
                        {
                            if (selectedTiles.Contains(clickedTile))
                            {
                                DeselectTile(clickedTile);
                            }
                            else
                            {
                                SelectTile(clickedTile);
                            }
                        }
                        else
                        {
                            if (selectedTiles.Count == 1 && selectedTiles.Contains(clickedTile))
                            {
                                DeselectAllTiles();
                            }
                            else
                            {
                                DeselectAllTiles();
                                SelectTile(clickedTile);
                            }
                        }
                    }
                }
                else
                {
                    if (selectedTiles.Count > 0)
                    {
                        MoveSelectedTilesToCell(cell);
                    }
                    else
                    {
                        PlaceAtCell(cell);
                    }
                }
            }

            if (allowDeleteWithRightClick && Mouse.current.rightButton.wasPressedThisFrame)
            {
                if (_byCell.TryGetValue(cell, out GameObject clickedObj))
                {
                    PlacedTile clickedTile = clickedObj.GetComponent<PlacedTile>();
                    if (clickedTile != null && selectedTiles.Contains(clickedTile))
                    {
                        RemoveSelectedTiles();
                    }
                    else
                    {
                        RemoveAtCell(cell);
                    }
                }
                else if (selectedTiles.Count > 0)
                {
                    DeselectAllTiles();
                }
                else
                {
                    RemoveAtCell(cell);
                }
            }
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
                PlacedTile tile = obj.GetComponent<PlacedTile>();
                if (tile != null && selectedTiles.Contains(tile))
                {
                    DeselectTile(tile);
                }

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
                    if (selectedTiles.Contains(m))
                    {
                        DeselectTile(m);
                    }

                    Destroy(m.gameObject);
                    break;
                }
            }
        }

        public void ClearAll()
        {
            DeselectAllTiles();

            foreach (var kv in _byCell) if (kv.Value != null) Destroy(kv.Value);
            _byCell.Clear();

            var markers = Object.FindObjectsByType<PlacedTile>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            foreach (var m in markers) if (m != null) Destroy(m.gameObject);
        }

        public void RebuildFrom(MapDataDTO data)
        {
            DeselectAllTiles();

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

        private void SelectTile(PlacedTile tile)
        {
            selectedTiles.Add(tile);

            if (referenceTile == null)
            {
                referenceTile = tile;
            }

            var renderer = tile.GetComponentInChildren<Renderer>();
            if (renderer != null && highlightMaterial != null)
            {
                originalMaterials[tile] = renderer.material;
                renderer.material = highlightMaterial;
            }

            UpdatePreviewHighlight();
            UpdateMultiSelectionPreviews();
        }

        private void DeselectTile(PlacedTile tile)
        {
            if (!selectedTiles.Contains(tile)) return;

            selectedTiles.Remove(tile);

            if (referenceTile == tile)
            {
                referenceTile = null;
                foreach (var t in selectedTiles)
                {
                    referenceTile = t;
                    break;
                }
            }

            var renderer = tile.GetComponentInChildren<Renderer>();
            if (renderer != null && originalMaterials.TryGetValue(tile, out Material original))
            {
                renderer.material = original;
                originalMaterials.Remove(tile);
            }

            UpdatePreviewHighlight();
            UpdateMultiSelectionPreviews();
        }

        private void DeselectAllTiles()
        {
            foreach (var tile in selectedTiles)
            {
                var renderer = tile.GetComponentInChildren<Renderer>();
                if (renderer != null && originalMaterials.TryGetValue(tile, out Material original))
                {
                    renderer.material = original;
                }
            }

            selectedTiles.Clear();
            originalMaterials.Clear();
            referenceTile = null;

            UpdatePreviewHighlight();
            ClearAdditionalPreviews();

            if (preview != null)
            {
                preview.SetActive(true);
            }
        }

        private void UpdatePreviewHighlight()
        {
            if (preview == null) return;

            var previewRenderer = preview.GetComponentInChildren<Renderer>();
            if (previewRenderer == null) return;

            if (selectedTiles.Count > 0)
            {
                if (highlightMaterial != null && previewOriginalMaterial == null)
                {
                    previewOriginalMaterial = previewRenderer.material;
                    previewRenderer.material = highlightMaterial;
                }
            }
            else
            {
                if (previewOriginalMaterial != null)
                {
                    previewRenderer.material = previewOriginalMaterial;
                    previewOriginalMaterial = null;
                }
            }
        }

        private void RemoveSelectedTiles()
        {
            var tilesToRemove = new List<PlacedTile>(selectedTiles);
            foreach (var tile in tilesToRemove)
            {
                _byCell.Remove(tile.cell);
                Destroy(tile.gameObject);
            }

            selectedTiles.Clear();
            originalMaterials.Clear();
            UpdatePreviewHighlight();
            ClearAdditionalPreviews();
        }

        private void MoveSelectedTilesToCell(Vector3Int targetCell)
        {
            if (selectedTiles.Count == 0 || referenceTile == null) return;

            Vector3 targetWorld = grid.GetCellCenterWorld(targetCell);
            Vector3 deltaWorld = targetWorld - referenceTile.transform.position;

            var tileMoves = new List<(PlacedTile tile, Vector3Int newCell, Vector3 newWorldPos)>();
            foreach (var tile in selectedTiles)
            {
                Vector3 newWorldPos = tile.transform.position + deltaWorld;
                Vector3Int newCell = grid.WorldToCell(newWorldPos);

                if (_byCell.ContainsKey(newCell))
                {
                    return;
                }

                tileMoves.Add((tile, newCell, newWorldPos));
            }

            foreach (var (tile, newCell, _) in tileMoves)
            {
                _byCell.Remove(tile.cell);
            }

            foreach (var (tile, newCell, newWorldPos) in tileMoves)
            {
                Vector3 worldPos = grid.GetCellCenterWorld(newCell);
                tile.transform.position = worldPos;
                tile.cell = newCell;
                _byCell[newCell] = tile.gameObject;
            }

            DeselectAllTiles();
        }

        private void UpdateMultiSelectionPreviews()
        {
            ClearAdditionalPreviews();

            if (selectedTiles.Count <= 1 || preview == null || referenceTile == null) return;

            foreach (var tile in selectedTiles)
            {
                if (tile == referenceTile) continue;

                var previewObj = Instantiate(preview);
                previewObj.name = $"AdditionalPreview_{tile.cell}";
                previewObj.transform.rotation = tile.transform.rotation;

                additionalPreviews.Add(previewObj);
            }
        }

        private void UpdateAdditionalPreviews(Vector3Int cursorCell)
        {
            if (selectedTiles.Count <= 1 || additionalPreviews.Count == 0 || referenceTile == null) return;

            Vector3 cursorWorld = grid.GetCellCenterWorld(cursorCell);
            Vector3 deltaWorld = cursorWorld - referenceTile.transform.position;

            int previewIndex = 0;
            foreach (var tile in selectedTiles)
            {
                if (tile == referenceTile) continue;

                if (previewIndex >= additionalPreviews.Count) break;

                Vector3 targetWorld = tile.transform.position + deltaWorld;

                additionalPreviews[previewIndex].transform.position = targetWorld;
                additionalPreviews[previewIndex].transform.rotation = tile.transform.rotation;

                previewIndex++;
            }
        }

        private void ClearAdditionalPreviews()
        {
            foreach (var previewObj in additionalPreviews)
            {
                if (previewObj != null) Destroy(previewObj);
            }
            additionalPreviews.Clear();
        }

        private bool CanPlaceAllSelectedTilesAt(Vector3Int targetCell)
        {
            if (selectedTiles.Count == 0) return true;
            if (referenceTile == null) return true;

            Vector3 targetWorld = grid.GetCellCenterWorld(targetCell);
            Vector3 deltaWorld = targetWorld - referenceTile.transform.position;

            foreach (var tile in selectedTiles)
            {
                Vector3 newWorldPos = tile.transform.position + deltaWorld;
                Vector3Int newCell = grid.WorldToCell(newWorldPos);

                if (_byCell.ContainsKey(newCell))
                {
                    return false;
                }
            }

            return true;
        }

        private void SetPreviewsVisibility(bool visible)
        {
            if (preview != null)
            {
                preview.SetActive(visible);
            }

            if (selectedTiles.Count == 0) return;

            foreach (var previewObj in additionalPreviews)
            {
                if (previewObj != null)
                {
                    previewObj.SetActive(visible);
                }
            }
        }

        private void MoveSelectedTilesInDirection(Vector3Int direction)
        {
            if (selectedTiles.Count == 0 || referenceTile == null) return;

            const int maxAttempts = 10;
            Vector3Int currentTargetCell = referenceTile.cell;

            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                currentTargetCell += direction;

                if (CanPlaceAllAtCell(currentTargetCell, ignoreSelectedTiles: true))
                {
                    ApplyMoveToCell(currentTargetCell);
                    return;
                }
            }
        }

        private bool CanPlaceAllAtCell(Vector3Int targetCell, bool ignoreSelectedTiles)
        {
            if (selectedTiles.Count == 0 || referenceTile == null) return false;

            Vector3 targetWorld = grid.GetCellCenterWorld(targetCell);
            Vector3 deltaWorld = targetWorld - referenceTile.transform.position;

            foreach (var tile in selectedTiles)
            {
                Vector3 newWorldPos = tile.transform.position + deltaWorld;
                Vector3Int newCell = grid.WorldToCell(newWorldPos);

                if (_byCell.ContainsKey(newCell))
                {
                    if (ignoreSelectedTiles)
                    {
                        GameObject occupyingObj = _byCell[newCell];
                        PlacedTile occupyingTile = occupyingObj?.GetComponent<PlacedTile>();

                        if (occupyingTile != null && selectedTiles.Contains(occupyingTile))
                        {
                            continue;
                        }
                    }

                    return false;
                }
            }

            return true;
        }

        private void ApplyMoveToCell(Vector3Int targetCell)
        {
            Vector3 targetWorld = grid.GetCellCenterWorld(targetCell);
            Vector3 deltaWorld = targetWorld - referenceTile.transform.position;

            foreach (var tile in selectedTiles)
            {
                _byCell.Remove(tile.cell);
            }

            foreach (var tile in selectedTiles)
            {
                Vector3 newWorldPos = tile.transform.position + deltaWorld;
                Vector3Int newCell = grid.WorldToCell(newWorldPos);
                Vector3 worldPos = grid.GetCellCenterWorld(newCell);

                tile.transform.position = worldPos;
                tile.cell = newCell;
                _byCell[newCell] = tile.gameObject;
            }

            hidePreviewsUntilMouseMove = true;
            SetPreviewsVisibility(false);
        }
    }
}
