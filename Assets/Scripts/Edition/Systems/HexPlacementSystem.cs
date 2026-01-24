using System.Collections.Generic;
using HexGrid.Models;
using UnityEngine;
using UnityEngine.InputSystem;

namespace HexGrid.Systems
{
    /// Main controller for hex tile placement, selection and interaction.
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

        /// External callback to block input (e.g., when hovering UI).
        public System.Func<bool> ShouldBlockInput;

        private const float HEX_ROTATION_ANGLE = 60f;
        private const float RAYCAST_MAX_DISTANCE = 1000f;
        private const int MAX_KEYBOARD_MOVE_ATTEMPTS = 10;

        private HexGridManager _gridManager;
        private TilePreviewManager _previewManager;
        private TileSelectionManager _selectionManager;
        // private HexTerrainLayoutViewModel _terrainViewModel;
        private Vector2 lastMousePosition;

        [Header("Keyboard Movement")]
        [SerializeField] private float initialKeyDelay = 0.3f;
        [SerializeField] private float keyRepeatInterval = 0.1f;
        private float keyRepeatTimer = 0f;
        private Vector3Int? lastKeyDirection = null;

        public int PrefabCount => _gridManager?.PrefabCount ?? 0;
        public bool HasGrid => _gridManager?.HasGrid ?? false;

        [Header("Map Bounds (recommended)")]
        [SerializeField] private Collider mapBoundsCollider; // drag your Plane collider here
        [SerializeField] private float boundsPadding = 0.02f; // small margin to avoid edge jitter
        [SerializeField] private bool blockPlacementOutsideBounds = true;

        private void Awake()
        {
            // var hexTerrainLayout = new HexterrainLayout{ Width = 100, Height = 100 };
            // _terrainViewModel = new HexTerrainLayoutViewModel(hexTerrainLayout);
            _gridManager = new HexGridManager(grid, tilePrefabs);
            _previewManager = new TilePreviewManager(preview, _gridManager);
            _selectionManager = new TileSelectionManager(_gridManager, _previewManager, moveSelectionMaterial, rotateSelectionMaterial, preview);
        }

        /// Handles key repeat timing for smooth continuous keyboard input.
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

            // Block tile interaction when camera controls are active
            bool cameraModifierPressed = Keyboard.current.leftAltKey.isPressed ||
                                         Keyboard.current.rightAltKey.isPressed ||
                                         Keyboard.current.leftShiftKey.isPressed ||
                                         Keyboard.current.rightShiftKey.isPressed;
            if (cameraModifierPressed)
            {
                _previewManager.SetPreviewsVisibility(false, _selectionManager.SelectionCount);
                return;
            }

            HandleModeInput();

            if (HandleSelectionInput()) return;

            if (HandleKeyboardMovement()) return;

            Vector2 mouse = Mouse.current.position.ReadValue();

            if (_previewManager.HidePreviewsUntilMouseMove && mouse != lastMousePosition)
            {
                _previewManager.HidePreviewsUntilMouseMove = false;
            }
            lastMousePosition = mouse;

            Ray ray = Camera.main.ScreenPointToRay(mouse);
            if (!Physics.Raycast(ray, out var hit, RAYCAST_MAX_DISTANCE, ~0)) return;

            if ((groundMask & (1 << hit.collider.gameObject.layer)) == 0) return;
            if (!HasGrid) { Debug.LogError("HexPlacementSystem: Grid is null."); return; }

            Vector3Int cell = _gridManager.WorldToCell(hit.point);
            Vector3 world = _gridManager.GetCellCenterWorld(cell);

            HandleMouseInput(cell, world);
        }

        private void HandleModeInput()
        {
            if (Keyboard.current.digit1Key.wasPressedThisFrame)
            {
                _selectionManager.SwitchSelectionMode(SelectionMode.Move);
            }
            else if (Keyboard.current.digit2Key.wasPressedThisFrame)
            {
                _selectionManager.SwitchSelectionMode(SelectionMode.Rotate);
            }
        }

        private bool HandleSelectionInput()
        {
            if (Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame)
            {
                _selectionManager.DeselectAllTiles();
                return true;
            }

            if (Keyboard.current.deleteKey.wasPressedThisFrame && _selectionManager.SelectionCount > 0)
            {
                _selectionManager.RemoveSelectedTiles();
                return true;
            }

            return false;
        }

        private bool HandleKeyboardMovement()
        {
            if (_selectionManager.SelectionCount > 0)
            {
                if (_selectionManager.CurrentMode == SelectionMode.Move)
                {
                    Vector3Int? currentDirection = null;
                    if (Keyboard.current.upArrowKey.isPressed) currentDirection = new Vector3Int(0, 1, 0);
                    else if (Keyboard.current.downArrowKey.isPressed) currentDirection = new Vector3Int(0, -1, 0);
                    else if (Keyboard.current.rightArrowKey.isPressed) currentDirection = new Vector3Int(1, 0, 0);
                    else if (Keyboard.current.leftArrowKey.isPressed) currentDirection = new Vector3Int(-1, 0, 0);

                    if (HandleKeyRepeat(currentDirection, () => _selectionManager.MoveInDirection(currentDirection.Value, MAX_KEYBOARD_MOVE_ATTEMPTS)))
                    {
                        return true;
                    }
                }
                else if (_selectionManager.CurrentMode == SelectionMode.Rotate)
                {
                    float? currentRotation = null;
                    if (Keyboard.current.leftArrowKey.isPressed) currentRotation = -HEX_ROTATION_ANGLE;
                    else if (Keyboard.current.rightArrowKey.isPressed) currentRotation = HEX_ROTATION_ANGLE;

                    Vector3Int? fakeDirection = currentRotation.HasValue
                        ? new Vector3Int(currentRotation.Value > 0 ? 1 : -1, 0, 0)
                        : null;

                    if (HandleKeyRepeat(fakeDirection, () => _selectionManager.RotateSelectedTiles(currentRotation.Value)))
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
            if (_selectionManager.CurrentMode == SelectionMode.Move)
            {
                _previewManager.SetPreviewPosition(world);

                bool canPlaceAll = _selectionManager.CanPlaceAllAtCell(cell, ignoreSelectedTiles: false);

                if (!_previewManager.HidePreviewsUntilMouseMove)
                {
                    _previewManager.SetPreviewsVisibility(canPlaceAll, _selectionManager.SelectionCount);
                    _previewManager.UpdateAdditionalPreviews(cell, _selectionManager.SelectedTiles, _selectionManager.ReferenceTile);
                }
            }
            else if (_selectionManager.CurrentMode == SelectionMode.Rotate)
            {
                _previewManager.SetPreviewsVisibility(false, _selectionManager.SelectionCount);
            }

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                bool ctrlHeld = Keyboard.current.leftCtrlKey.isPressed || Keyboard.current.rightCtrlKey.isPressed;

                if (_gridManager.TryGetTileAt(cell, out GameObject tileObj))
                {
                    PlacedTile clickedTile = tileObj.GetComponent<PlacedTile>();
                    if (clickedTile != null)
                    {
                        if (ctrlHeld)
                        {
                            if (_selectionManager.Contains(clickedTile))
                            {
                                _selectionManager.DeselectTile(clickedTile);
                            }
                            else
                            {
                                _selectionManager.SelectTile(clickedTile);
                            }
                        }
                        else
                        {
                            if (_selectionManager.SelectionCount == 1 && _selectionManager.Contains(clickedTile))
                            {
                                _selectionManager.DeselectAllTiles();
                            }
                            else
                            {
                                _selectionManager.DeselectAllTiles(resetModeToMove: false);
                                _selectionManager.SelectTile(clickedTile);
                            }
                        }
                    }
                }
                else
                {
                    if (_selectionManager.SelectionCount > 0)
                    {
                        if (_selectionManager.CurrentMode == SelectionMode.Move)
                        {
                            _selectionManager.MoveTilesToCell(cell, validateCollisions: true, hidePreviewsAfter: false);
                        }
                    }
                    else
                    {
                        _gridManager.PlaceAtCell(cell, currentPrefabIndex, _previewManager.GetPreviewYRotation());
                    }
                }
            }

            if (allowDeleteWithRightClick && Mouse.current.rightButton.wasPressedThisFrame)
            {
                if (_gridManager.TryGetTileAt(cell, out GameObject clickedObj))
                {
                    PlacedTile clickedTile = clickedObj.GetComponent<PlacedTile>();
                    if (clickedTile != null && _selectionManager.Contains(clickedTile))
                    {
                        _selectionManager.RemoveSelectedTiles();
                    }
                    else
                    {
                        _gridManager.RemoveAtCell(cell);
                    }
                }
                else if (_selectionManager.SelectionCount > 0)
                {
                    _selectionManager.DeselectAllTiles();
                }
                else
                {
                    _gridManager.RemoveAtCell(cell);
                }
            }
        }

        public void ClearAll()
        {
            _selectionManager.DeselectAllTiles();
            _gridManager.ClearAll();
        }

        public void RebuildFrom(MapDataDTO data)
        {
            _selectionManager.DeselectAllTiles();
            _gridManager.RebuildFrom(data);
        }
    }
}
