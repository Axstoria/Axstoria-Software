using System.Collections.Generic;
using HexGrid.Models;
using UnityEngine;

namespace HexGrid.Systems
{
    public enum SelectionMode
    {
        Move,
        Rotate
    }

    /// Manages tile selection state, materials, movement and rotation.
    public class TileSelectionManager
    {
        private readonly HashSet<PlacedTile> _selectedTiles = new();
        /// Stores original materials to restore when deselecting.
        private readonly Dictionary<PlacedTile, Material> _originalMaterials = new();
        private readonly HexGridManager _gridManager;
        private readonly TilePreviewManager _previewManager;
        private readonly Material _moveSelectionMaterial;
        private readonly Material _rotateSelectionMaterial;
        private readonly GameObject _preview;

        private PlacedTile _referenceTile;
        private SelectionMode _currentSelectionMode = SelectionMode.Move;

        public int SelectionCount => _selectedTiles.Count;
        public bool HasSelection => _selectedTiles.Count > 0;
        /// First selected tile, used as anchor for multi-tile movement.
        public PlacedTile ReferenceTile => _referenceTile;
        public SelectionMode CurrentMode => _currentSelectionMode;
        public HashSet<PlacedTile> SelectedTiles => _selectedTiles;

        private Material CurrentSelectionMaterial => _currentSelectionMode == SelectionMode.Move
            ? _moveSelectionMaterial
            : _rotateSelectionMaterial;

        public TileSelectionManager(
            HexGridManager gridManager,
            TilePreviewManager previewManager,
            Material moveSelectionMaterial,
            Material rotateSelectionMaterial,
            GameObject preview)
        {
            _gridManager = gridManager;
            _previewManager = previewManager;
            _moveSelectionMaterial = moveSelectionMaterial;
            _rotateSelectionMaterial = rotateSelectionMaterial;
            _preview = preview;
        }

        public void SelectTile(PlacedTile tile)
        {
            _selectedTiles.Add(tile);

            if (_referenceTile == null)
            {
                _referenceTile = tile;
            }

            var renderer = tile.GetComponentInChildren<Renderer>();
            if (renderer != null)
            {
                if (CurrentSelectionMaterial != null)
                {
                    _originalMaterials[tile] = renderer.material;
                    renderer.material = CurrentSelectionMaterial;
                }
                else
                {
                    Debug.LogWarning("HexPlacementSystem: Selection material not assigned. Please assign moveSelectionMaterial and rotateSelectionMaterial in Inspector.");
                }
            }

            _previewManager.UpdatePreviewHighlight(CurrentSelectionMaterial, _selectedTiles.Count > 0);
            _previewManager.UpdateMultiSelectionPreviews(_selectedTiles, _referenceTile);
        }

        public void DeselectTile(PlacedTile tile)
        {
            if (!_selectedTiles.Contains(tile)) return;

            _selectedTiles.Remove(tile);

            if (_referenceTile == tile)
            {
                _referenceTile = null;
                foreach (var t in _selectedTiles)
                {
                    _referenceTile = t;
                    break;
                }
            }

            RestoreTileMaterial(tile);

            _previewManager.UpdatePreviewHighlight(CurrentSelectionMaterial, _selectedTiles.Count > 0);
            _previewManager.UpdateMultiSelectionPreviews(_selectedTiles, _referenceTile);
        }

        public void DeselectAllTiles(bool resetModeToMove = true)
        {
            foreach (var tile in _selectedTiles)
            {
                RestoreTileMaterial(tile, removeFromDictionary: false);
            }

            _selectedTiles.Clear();
            _originalMaterials.Clear();
            _referenceTile = null;

            if (resetModeToMove)
            {
                _currentSelectionMode = SelectionMode.Move;
            }

            _previewManager.UpdatePreviewHighlight(CurrentSelectionMaterial, _selectedTiles.Count > 0);
            _previewManager.ClearAdditionalPreviews();

            if (_preview != null)
            {
                _preview.SetActive(true);
            }

            if (resetModeToMove)
            {
                _previewManager.SetPreviewsVisibility(true, _selectedTiles.Count);
            }
        }

        public void RemoveSelectedTiles()
        {
            var tilesToRemove = new List<PlacedTile>(_selectedTiles);
            foreach (var tile in tilesToRemove)
            {
                _gridManager.RemoveAtCell(tile.cell);
            }

            _selectedTiles.Clear();
            _originalMaterials.Clear();
            _referenceTile = null;

            _currentSelectionMode = SelectionMode.Move;

            _previewManager.UpdatePreviewHighlight(CurrentSelectionMaterial, _selectedTiles.Count > 0);
            _previewManager.ClearAdditionalPreviews();

            if (_preview != null)
            {
                _preview.SetActive(true);
            }

            _previewManager.SetPreviewsVisibility(true, _selectedTiles.Count);
        }

        public bool MoveTilesToCell(Vector3Int targetCell, bool validateCollisions, bool hidePreviewsAfter)
        {
            if (_selectedTiles.Count == 0 || _referenceTile == null) return false;

            Vector3 targetWorld = _gridManager.GetCellCenterWorld(targetCell);
            Vector3 deltaWorld = targetWorld - _referenceTile.transform.position;

            if (validateCollisions)
            {
                foreach (var tile in _selectedTiles)
                {
                    Vector3 newWorldPos = tile.transform.position + deltaWorld;
                    Vector3Int newCell = _gridManager.WorldToCell(newWorldPos);
                    if (_gridManager.ContainsCell(newCell)) return false;
                }
            }

            foreach (var tile in _selectedTiles)
            {
                Vector3Int newCell = _gridManager.WorldToCell(tile.transform.position + deltaWorld);
                Vector3 worldPos = _gridManager.GetCellCenterWorld(newCell);

                _gridManager.UpdateTilePosition(tile, tile.cell, newCell);
                tile.transform.position = worldPos;
                tile.cell = newCell;
            }

            if (hidePreviewsAfter)
            {
                _previewManager.HidePreviewsUntilMouseMove = true;
                _previewManager.SetPreviewsVisibility(false, _selectedTiles.Count);
            }

            return true;
        }

        public void RotateSelectedTiles(float angleDelta)
        {
            foreach (var tile in _selectedTiles)
            {
                tile.transform.Rotate(0f, angleDelta, 0f, Space.Self);
                tile.yRotation = tile.transform.eulerAngles.y;
            }

            _previewManager.RotateAdditionalPreviews(angleDelta);

            if (_selectedTiles.Count > 0)
            {
                _previewManager.RotatePreview(angleDelta);
            }
        }

        public void SwitchSelectionMode(SelectionMode newMode)
        {
            if (_currentSelectionMode == newMode) return;

            _currentSelectionMode = newMode;

            foreach (var tile in _selectedTiles)
            {
                var renderer = tile.GetComponentInChildren<Renderer>();
                if (renderer != null && CurrentSelectionMaterial != null)
                {
                    renderer.material = CurrentSelectionMaterial;
                }
            }

            _previewManager.UpdatePreviewHighlight(CurrentSelectionMaterial, _selectedTiles.Count > 0);
            _previewManager.UpdateMultiSelectionPreviews(_selectedTiles, _referenceTile);
        }

        public bool Contains(PlacedTile tile)
        {
            return _selectedTiles.Contains(tile);
        }

        /// Moves selection in direction, skipping occupied cells up to maxAttempts.
        public void MoveInDirection(Vector3Int direction, int maxAttempts)
        {
            if (_selectedTiles.Count == 0 || _referenceTile == null) return;

            Vector3Int currentTargetCell = _referenceTile.cell;

            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                currentTargetCell += direction;

                if (CanPlaceAllAtCell(currentTargetCell, ignoreSelectedTiles: true))
                {
                    MoveTilesToCell(currentTargetCell, validateCollisions: false, hidePreviewsAfter: true);
                    return;
                }
            }
        }

        public bool CanPlaceAllAtCell(Vector3Int targetCell, bool ignoreSelectedTiles)
        {
            if (_selectedTiles.Count == 0 || _referenceTile == null)
            {
                return !ignoreSelectedTiles;
            }

            Vector3 deltaWorld = CalculateDeltaWorld(targetCell);

            foreach (var tile in _selectedTiles)
            {
                Vector3 newWorldPos = tile.transform.position + deltaWorld;
                Vector3Int newCell = _gridManager.WorldToCell(newWorldPos);

                if (_gridManager.ContainsCell(newCell))
                {
                    if (ignoreSelectedTiles)
                    {
                        _gridManager.TryGetTileAt(newCell, out GameObject occupyingObj);
                        PlacedTile occupyingTile = occupyingObj?.GetComponent<PlacedTile>();

                        if (occupyingTile != null && _selectedTiles.Contains(occupyingTile))
                        {
                            continue;
                        }
                    }

                    return false;
                }
            }

            return true;
        }

        private Vector3 CalculateDeltaWorld(Vector3Int targetCell)
        {
            Vector3 targetWorld = _gridManager.GetCellCenterWorld(targetCell);
            return targetWorld - _referenceTile.transform.position;
        }

        private void RestoreTileMaterial(PlacedTile tile, bool removeFromDictionary = true)
        {
            var renderer = tile.GetComponentInChildren<Renderer>();
            if (renderer != null && _originalMaterials.TryGetValue(tile, out Material original))
            {
                renderer.material = original;
                if (removeFromDictionary)
                {
                    _originalMaterials.Remove(tile);
                }
            }
        }
    }
}
