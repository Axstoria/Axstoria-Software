using System.Collections.Generic;
using Controler.Editor.ViewModels;
using Domain;
using Edition.Models;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Edition.Systems
{
    /// Main controller for hex tile placement, selection and interaction.
    public class HexPlacementSystem : MonoBehaviour
    {
        [Header("Grid & Prefabs")]
        [SerializeField] private Grid grid;
        [SerializeField] private List<GameObject> tilePrefabs = new();
        [SerializeField] private int currentPrefabIndex;

        [Header("Placement")]
        [SerializeField] private LayerMask groundMask;
        [SerializeField] private GameObject preview;
        [SerializeField] private bool allowDeleteWithRightClick = true;

        [Header("Selection")]
        [SerializeField] private Material moveSelectionMaterial;
        [SerializeField] private Material rotateSelectionMaterial;

        /// External callback to block input (e.g., when hovering UI).
        public System.Func<bool> shouldBlockInput;

        private const float HEX_ROTATION_ANGLE = 60f;
        private const float RAYCAST_MAX_DISTANCE = 1000f;
        private const int MAX_KEYBOARD_MOVE_ATTEMPTS = 10;

        private HexGridManager gridManager;
        private TilePreviewManager previewManager;
        private TileSelectionManager selectionManager;

        private Vector2 lastMousePosition;

        [Header("Keyboard Movement")]
        [SerializeField] private float initialKeyDelay = 0.3f;
        [SerializeField] private float keyRepeatInterval = 0.1f;
        private float keyRepeatTimer;
        private Vector3Int? lastKeyDirection;

        public int PrefabCount => gridManager?.PrefabCount ?? 0;
        public bool HasGrid => gridManager?.HasGrid ?? false;

        [Header("Map Bounds (recommended)")]
        [SerializeField] private Collider mapBoundsCollider; // drag your Plane collider here
        [SerializeField] private float boundsPadding = 0.02f; // small margin to avoid edge jitter
        [SerializeField] private bool blockPlacementOutsideBounds = true;

        private void Awake()
        {
            gridManager = new HexGridManager(grid, tilePrefabs);
            var terrainLayout = new TerrainLayout
            {
                Width = 100,
                Height = 100,
            };
            var terrainLayoutVm = new TerrainLayoutViewModel(terrainLayout);
            
            gridManager.SetTerrainLayout(terrainLayoutVm);
            previewManager = new TilePreviewManager(preview, gridManager);
            selectionManager = new TileSelectionManager(gridManager, previewManager, moveSelectionMaterial, rotateSelectionMaterial, preview);
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
            if (shouldBlockInput != null && shouldBlockInput()) return;

            // Block tile interaction when camera controls are active
            bool cameraModifierPressed = Keyboard.current.leftAltKey.isPressed ||
                                         Keyboard.current.rightAltKey.isPressed ||
                                         Keyboard.current.leftShiftKey.isPressed ||
                                         Keyboard.current.rightShiftKey.isPressed;
            if (cameraModifierPressed)
            {
                previewManager.SetPreviewsVisibility(false, selectionManager.SelectionCount);
                return;
            }

            HandleModeInput();

            if (HandleSelectionInput()) return;

            if (HandleKeyboardMovement()) return;

            Vector2 mouse = Mouse.current.position.ReadValue();

            if (previewManager.HidePreviewsUntilMouseMove && mouse != lastMousePosition)
            {
                previewManager.HidePreviewsUntilMouseMove = false;
            }
            lastMousePosition = mouse;

            Ray ray = Camera.main.ScreenPointToRay(mouse);
            if (!Physics.Raycast(ray, out var hit, RAYCAST_MAX_DISTANCE, ~0)) return;

            if ((groundMask & (1 << hit.collider.gameObject.layer)) == 0) return;
            if (!HasGrid) { Debug.LogError("HexPlacementSystem: Grid is null."); return; }

            Vector3Int cell = gridManager.WorldToCell(hit.point);
            Vector3 world = gridManager.GetCellCenterWorld(cell);

            HandleMouseInput(cell, world);
        }

        private void HandleModeInput()
        {
            if (Keyboard.current.digit1Key.wasPressedThisFrame)
            {
                selectionManager.SwitchSelectionMode(SelectionMode.Move);
            }
            else if (Keyboard.current.digit2Key.wasPressedThisFrame)
            {
                selectionManager.SwitchSelectionMode(SelectionMode.Rotate);
            }
        }

        private bool HandleSelectionInput()
        {
            if (Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame)
            {
                selectionManager.DeselectAllTiles();
                return true;
            }

            if (Keyboard.current.deleteKey.wasPressedThisFrame && selectionManager.SelectionCount > 0)
            {
                selectionManager.RemoveSelectedTiles();
                return true;
            }

            return false;
        }

        private bool HandleKeyboardMovement()
        {
            if (selectionManager.SelectionCount > 0)
            {
                if (selectionManager.CurrentMode == SelectionMode.Move)
                {
                    Vector3Int? currentDirection = null;
                    if (Keyboard.current.upArrowKey.isPressed) currentDirection = new Vector3Int(0, 1, 0);
                    else if (Keyboard.current.downArrowKey.isPressed) currentDirection = new Vector3Int(0, -1, 0);
                    else if (Keyboard.current.rightArrowKey.isPressed) currentDirection = new Vector3Int(1, 0, 0);
                    else if (Keyboard.current.leftArrowKey.isPressed) currentDirection = new Vector3Int(-1, 0, 0);

                    if (HandleKeyRepeat(currentDirection, () =>
                        {
                            if (currentDirection != null)
                                selectionManager.MoveInDirection(currentDirection.Value, MAX_KEYBOARD_MOVE_ATTEMPTS);
                        }))
                    {
                        return true;
                    }
                }
                else if (selectionManager.CurrentMode == SelectionMode.Rotate)
                {
                    float? currentRotation = null;
                    if (Keyboard.current.leftArrowKey.isPressed) currentRotation = -HEX_ROTATION_ANGLE;
                    else if (Keyboard.current.rightArrowKey.isPressed) currentRotation = HEX_ROTATION_ANGLE;

                    Vector3Int? fakeDirection = currentRotation.HasValue
                        ? new Vector3Int(currentRotation.Value > 0 ? 1 : -1, 0, 0)
                        : null;

                    if (HandleKeyRepeat(fakeDirection, () =>
                        {
                            if (currentRotation != null) selectionManager.RotateSelectedTiles(currentRotation.Value);
                        }))
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
            if (selectionManager.CurrentMode == SelectionMode.Move)
            {
                previewManager.SetPreviewPosition(world);

                bool canPlaceAll = selectionManager.CanPlaceAllAtCell(cell, ignoreSelectedTiles: false);

                if (!previewManager.HidePreviewsUntilMouseMove)
                {
                    previewManager.SetPreviewsVisibility(canPlaceAll, selectionManager.SelectionCount);
                    previewManager.UpdateAdditionalPreviews(cell, selectionManager.SelectedTiles, selectionManager.ReferenceTile);
                }
            }
            else if (selectionManager.CurrentMode == SelectionMode.Rotate)
            {
                previewManager.SetPreviewsVisibility(false, selectionManager.SelectionCount);
            }

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                bool ctrlHeld = Keyboard.current.leftCtrlKey.isPressed || Keyboard.current.rightCtrlKey.isPressed;

                if (gridManager.TryGetTileAt(cell, out GameObject tileObj))
                {
                    PlacedTile clickedTile = tileObj.GetComponent<PlacedTile>();
                    if (clickedTile != null)
                    {
                        if (ctrlHeld)
                        {
                            if (selectionManager.Contains(clickedTile))
                            {
                                selectionManager.DeselectTile(clickedTile);
                            }
                            else
                            {
                                selectionManager.SelectTile(clickedTile);
                            }
                        }
                        else
                        {
                            if (selectionManager.SelectionCount == 1 && selectionManager.Contains(clickedTile))
                            {
                                selectionManager.DeselectAllTiles();
                            }
                            else
                            {
                                selectionManager.DeselectAllTiles(resetModeToMove: false);
                                selectionManager.SelectTile(clickedTile);
                            }
                        }
                    }
                }
                else
                {
                    if (selectionManager.SelectionCount > 0)
                    {
                        if (selectionManager.CurrentMode == SelectionMode.Move)
                        {
                            selectionManager.MoveTilesToCell(cell, validateCollisions: true, hidePreviewsAfter: false);
                        }
                    }
                    else
                    {
                        gridManager.PlaceAtCell(cell, currentPrefabIndex, previewManager.GetPreviewYRotation());
                    }
                }
            }

            if (allowDeleteWithRightClick && Mouse.current.rightButton.wasPressedThisFrame)
            {
                if (gridManager.TryGetTileAt(cell, out GameObject clickedObj))
                {
                    PlacedTile clickedTile = clickedObj.GetComponent<PlacedTile>();
                    if (clickedTile != null && selectionManager.Contains(clickedTile))
                    {
                        selectionManager.RemoveSelectedTiles();
                    }
                    else
                    {
                        gridManager.RemoveAtCell(cell);
                    }
                }
                else if (selectionManager.SelectionCount > 0)
                {
                    selectionManager.DeselectAllTiles();
                }
                else
                {
                    gridManager.RemoveAtCell(cell);
                }
            }
        }

        public void ClearAll()
        {
            selectionManager.DeselectAllTiles();
            gridManager.ClearAll();
        }

        public void RebuildFrom(MapDataDTO data)
        {
            selectionManager.DeselectAllTiles();
            gridManager.RebuildFrom(data);
        }
    }
}