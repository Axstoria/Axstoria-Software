using System.Collections.Generic;
using HexGrid.Models;
using UnityEngine;
using UnityEngine.InputSystem;

namespace HexGrid.Systems
{
    public enum SelectionMode
    {
        Move,
        Rotate
    }

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
        [SerializeField] private Material moveSelectionMaterial;
        [SerializeField] private Material rotateSelectionMaterial;
        private SelectionMode currentSelectionMode = SelectionMode.Move;

        // External blocker (UI hover etc.)
        public System.Func<bool> ShouldBlockInput;

        private const float HEX_ROTATION_ANGLE = 60f;
        private const float RAYCAST_MAX_DISTANCE = 1000f;
        private const int MAX_KEYBOARD_MOVE_ATTEMPTS = 10;

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

        private Material CurrentSelectionMaterial => currentSelectionMode == SelectionMode.Move
            ? moveSelectionMaterial
            : rotateSelectionMaterial;

        private Vector3 CalculateDeltaWorld(Vector3Int targetCell)
        {
            Vector3 targetWorld = grid.GetCellCenterWorld(targetCell);
            return targetWorld - referenceTile.transform.position;
        }

        private bool HandleKeyRepeat(Vector3Int? currentDirection, System.Action action)
        {
            if (currentDirection.HasValue)
            {
                if (currentDirection != lastKeyDirection)
                {
                    action();
                    lastKeyDirection = currentDirection;
                    keyRepeatTimer = initialKeyDelay;
                }
                else
                {
                    keyRepeatTimer -= Time.deltaTime;
                    if (keyRepeatTimer <= 0f)
                    {
                        action();
                        keyRepeatTimer = keyRepeatInterval;
                    }
                }
                return true;
            }
            else
            {
                lastKeyDirection = null;
                keyRepeatTimer = 0f;
                return false;
            }
        }

        private void Update()
        {
            if (ShouldBlockInput != null && ShouldBlockInput()) return;

            HandleModeInput();

            if (HandleSelectionInput()) return;

            if (HandleKeyboardMovement()) return;

            Vector2 mouse = Mouse.current.position.ReadValue();

            if (hidePreviewsUntilMouseMove && mouse != lastMousePosition)
            {
                hidePreviewsUntilMouseMove = false;
            }
            lastMousePosition = mouse;

            Ray ray = Camera.main.ScreenPointToRay(mouse);
            if (!Physics.Raycast(ray, out var hit, RAYCAST_MAX_DISTANCE, ~0)) return;

            if ((groundMask & (1 << hit.collider.gameObject.layer)) == 0) return;
            if (grid == null) { Debug.LogError("HexPlacementSystem: Grid is null."); return; }

            Vector3Int cell = grid.WorldToCell(hit.point);
            Vector3 world = grid.GetCellCenterWorld(cell);

            HandleMouseInput(cell, world);
        }

        private void HandleModeInput()
        {
            if (Keyboard.current.digit1Key.wasPressedThisFrame)
            {
                SwitchSelectionMode(SelectionMode.Move);
            }
            else if (Keyboard.current.digit2Key.wasPressedThisFrame)
            {
                SwitchSelectionMode(SelectionMode.Rotate);
            }
        }

        private bool HandleSelectionInput()
        {
            if (Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame)
            {
                DeselectAllTiles();
                return true;
            }

            if (Keyboard.current.deleteKey.wasPressedThisFrame && selectedTiles.Count > 0)
            {
                RemoveSelectedTiles();
                return true;
            }

            return false;
        }

        private bool HandleKeyboardMovement()
        {
            if (selectedTiles.Count > 0)
            {
                if (currentSelectionMode == SelectionMode.Move)
                {
                    Vector3Int? currentDirection = null;
                    if (Keyboard.current.upArrowKey.isPressed) currentDirection = new Vector3Int(0, 1, 0);
                    else if (Keyboard.current.downArrowKey.isPressed) currentDirection = new Vector3Int(0, -1, 0);
                    else if (Keyboard.current.rightArrowKey.isPressed) currentDirection = new Vector3Int(1, 0, 0);
                    else if (Keyboard.current.leftArrowKey.isPressed) currentDirection = new Vector3Int(-1, 0, 0);

                    if (HandleKeyRepeat(currentDirection, () => MoveSelectedTilesInDirection(currentDirection.Value)))
                    {
                        return true;
                    }
                }
                else if (currentSelectionMode == SelectionMode.Rotate)
                {
                    float? currentRotation = null;
                    if (Keyboard.current.leftArrowKey.isPressed) currentRotation = -HEX_ROTATION_ANGLE;
                    else if (Keyboard.current.rightArrowKey.isPressed) currentRotation = HEX_ROTATION_ANGLE;

                    Vector3Int? fakeDirection = currentRotation.HasValue
                        ? new Vector3Int(currentRotation.Value > 0 ? 1 : -1, 0, 0)
                        : null;

                    if (HandleKeyRepeat(fakeDirection, () => RotateSelectedTiles(currentRotation.Value)))
                    {
                        return true;
                    }
                }
            }
            else
            {
                lastKeyDirection = null;
                keyRepeatTimer = 0f;
            }

            return false;
        }

        private void HandleMouseInput(Vector3Int cell, Vector3 world)
        {
            if (currentSelectionMode == SelectionMode.Move)
            {
                if (preview != null) preview.transform.position = world;

                bool canPlaceAll = CanPlaceAllAtCell(cell, ignoreSelectedTiles: false);

                if (!hidePreviewsUntilMouseMove)
                {
                    SetPreviewsVisibility(canPlaceAll);
                    UpdateAdditionalPreviews(cell);
                }
            }
            else if (currentSelectionMode == SelectionMode.Rotate)
            {
                SetPreviewsVisibility(false);
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
                                DeselectAllTiles(resetModeToMove: false);
                                SelectTile(clickedTile);
                            }
                        }
                    }
                }
                else
                {
                    if (selectedTiles.Count > 0)
                    {
                        if (currentSelectionMode == SelectionMode.Move)
                        {
                            MoveTilesToCell(cell, validateCollisions: true, hidePreviewsAfter: false);
                        }
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
            if (renderer != null)
            {
                if (CurrentSelectionMaterial != null)
                {
                    originalMaterials[tile] = renderer.material;
                    renderer.material = CurrentSelectionMaterial;
                }
                else
                {
                    Debug.LogWarning("HexPlacementSystem: Selection material not assigned. Please assign moveSelectionMaterial and rotateSelectionMaterial in Inspector.");
                }
            }

            UpdatePreviewHighlight();
            UpdateMultiSelectionPreviews();
        }

        private void RestoreTileMaterial(PlacedTile tile, bool removeFromDictionary = true)
        {
            var renderer = tile.GetComponentInChildren<Renderer>();
            if (renderer != null && originalMaterials.TryGetValue(tile, out Material original))
            {
                renderer.material = original;
                if (removeFromDictionary)
                {
                    originalMaterials.Remove(tile);
                }
            }
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

            RestoreTileMaterial(tile);

            UpdatePreviewHighlight();
            UpdateMultiSelectionPreviews();
        }

        private void DeselectAllTiles(bool resetModeToMove = true)
        {
            foreach (var tile in selectedTiles)
            {
                RestoreTileMaterial(tile, removeFromDictionary: false);
            }

            selectedTiles.Clear();
            originalMaterials.Clear();
            referenceTile = null;

            if (resetModeToMove)
            {
                currentSelectionMode = SelectionMode.Move;
            }

            UpdatePreviewHighlight();
            ClearAdditionalPreviews();

            if (preview != null)
            {
                preview.SetActive(true);
            }

            if (resetModeToMove)
            {
                SetPreviewsVisibility(true);
            }
        }

        private void UpdatePreviewHighlight()
        {
            if (preview == null) return;

            var previewRenderer = preview.GetComponentInChildren<Renderer>();
            if (previewRenderer == null) return;

            if (selectedTiles.Count > 0)
            {
                if (CurrentSelectionMaterial != null && previewOriginalMaterial == null)
                {
                    previewOriginalMaterial = previewRenderer.material;
                    previewRenderer.material = CurrentSelectionMaterial;
                }
                else if (CurrentSelectionMaterial != null && previewOriginalMaterial != null)
                {
                    previewRenderer.material = CurrentSelectionMaterial;
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
            referenceTile = null;

            currentSelectionMode = SelectionMode.Move;

            UpdatePreviewHighlight();
            ClearAdditionalPreviews();

            if (preview != null)
            {
                preview.SetActive(true);
            }

            SetPreviewsVisibility(true);
        }

        private bool MoveTilesToCell(Vector3Int targetCell, bool validateCollisions, bool hidePreviewsAfter)
        {
            if (selectedTiles.Count == 0 || referenceTile == null) return false;

            Vector3 deltaWorld = CalculateDeltaWorld(targetCell);

            if (validateCollisions)
            {
                foreach (var tile in selectedTiles)
                {
                    Vector3 newWorldPos = tile.transform.position + deltaWorld;
                    Vector3Int newCell = grid.WorldToCell(newWorldPos);
                    if (_byCell.ContainsKey(newCell)) return false;
                }
            }

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

            if (hidePreviewsAfter)
            {
                hidePreviewsUntilMouseMove = true;
                SetPreviewsVisibility(false);
            }

            return true;
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

            Vector3 deltaWorld = CalculateDeltaWorld(cursorCell);

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

            Vector3Int currentTargetCell = referenceTile.cell;

            for (int attempt = 1; attempt <= MAX_KEYBOARD_MOVE_ATTEMPTS; attempt++)
            {
                currentTargetCell += direction;

                if (CanPlaceAllAtCell(currentTargetCell, ignoreSelectedTiles: true))
                {
                    MoveTilesToCell(currentTargetCell, validateCollisions: false, hidePreviewsAfter: true);
                    return;
                }
            }
        }

        private bool CanPlaceAllAtCell(Vector3Int targetCell, bool ignoreSelectedTiles)
        {
            if (selectedTiles.Count == 0 || referenceTile == null)
            {
                return !ignoreSelectedTiles;
            }

            Vector3 deltaWorld = CalculateDeltaWorld(targetCell);

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

        private void SwitchSelectionMode(SelectionMode newMode)
        {
            if (currentSelectionMode == newMode) return;

            currentSelectionMode = newMode;

            foreach (var tile in selectedTiles)
            {
                var renderer = tile.GetComponentInChildren<Renderer>();
                if (renderer != null && CurrentSelectionMaterial != null)
                {
                    renderer.material = CurrentSelectionMaterial;
                }
            }

            UpdatePreviewHighlight();
            UpdateMultiSelectionPreviews();
        }

        private void RotateSelectedTiles(float angleDelta)
        {
            foreach (var tile in selectedTiles)
            {
                tile.transform.Rotate(0f, angleDelta, 0f, Space.Self);
                tile.yRotation = tile.transform.eulerAngles.y;
            }

            foreach (var previewObj in additionalPreviews)
            {
                if (previewObj != null)
                {
                    previewObj.transform.Rotate(0f, angleDelta, 0f, Space.Self);
                }
            }

            if (preview != null && selectedTiles.Count > 0)
            {
                preview.transform.Rotate(0f, angleDelta, 0f, Space.Self);
            }
        }
    }
}
