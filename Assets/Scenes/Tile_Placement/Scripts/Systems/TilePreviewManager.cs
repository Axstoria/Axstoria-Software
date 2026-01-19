using System.Collections.Generic;
using HexGrid.Models;
using UnityEngine;

namespace HexGrid.Systems
{
    /// Manages preview GameObjects: main preview and multi-selection previews.
    public class TilePreviewManager
    {
        private readonly GameObject _preview;
        private readonly List<GameObject> _additionalPreviews = new();
        private readonly HexGridManager _gridManager;

        private Material _previewOriginalMaterial;
        private bool _hidePreviewsUntilMouseMove;

        /// Gets or sets whether previews should be hidden until mouse moves.
        public bool HidePreviewsUntilMouseMove
        {
            get => _hidePreviewsUntilMouseMove;
            set => _hidePreviewsUntilMouseMove = value;
        }

        /// Initializes the preview manager with preview GameObject and grid manager.
        public TilePreviewManager(GameObject preview, HexGridManager gridManager)
        {
            _preview = preview;
            _gridManager = gridManager;
        }

        /// Updates preview material to match current selection mode.
        public void UpdatePreviewHighlight(Material selectionMaterial, bool hasSelection)
        {
            if (_preview == null) return;

            var previewRenderer = _preview.GetComponentInChildren<Renderer>();
            if (previewRenderer == null) return;

            if (hasSelection)
            {
                if (selectionMaterial != null && _previewOriginalMaterial == null)
                {
                    _previewOriginalMaterial = previewRenderer.material;
                    previewRenderer.material = selectionMaterial;
                }
                else if (selectionMaterial != null && _previewOriginalMaterial != null)
                {
                    previewRenderer.material = selectionMaterial;
                }
            }
            else
            {
                if (_previewOriginalMaterial != null)
                {
                    previewRenderer.material = _previewOriginalMaterial;
                    _previewOriginalMaterial = null;
                }
            }
        }

        /// Creates preview instances for each selected tile (except reference).
        public void UpdateMultiSelectionPreviews(HashSet<PlacedTile> selectedTiles, PlacedTile referenceTile)
        {
            ClearAdditionalPreviews();

            if (selectedTiles.Count <= 1 || _preview == null || referenceTile == null) return;

            foreach (var tile in selectedTiles)
            {
                if (tile == referenceTile) continue;

                var previewObj = Object.Instantiate(_preview);
                previewObj.name = $"AdditionalPreview_{tile.cell}";
                previewObj.transform.rotation = tile.transform.rotation;

                _additionalPreviews.Add(previewObj);
            }
        }

        /// Updates positions of additional previews to follow cursor.
        public void UpdateAdditionalPreviews(Vector3Int cursorCell, HashSet<PlacedTile> selectedTiles, PlacedTile referenceTile)
        {
            if (selectedTiles.Count <= 1 || _additionalPreviews.Count == 0 || referenceTile == null) return;

            Vector3 cursorWorld = _gridManager.GetCellCenterWorld(cursorCell);
            Vector3 deltaWorld = cursorWorld - referenceTile.transform.position;

            int previewIndex = 0;
            foreach (var tile in selectedTiles)
            {
                if (tile == referenceTile) continue;

                if (previewIndex >= _additionalPreviews.Count) break;

                Vector3 targetWorld = tile.transform.position + deltaWorld;

                _additionalPreviews[previewIndex].transform.position = targetWorld;
                _additionalPreviews[previewIndex].transform.rotation = tile.transform.rotation;

                previewIndex++;
            }
        }

        /// Destroys all additional preview instances.
        public void ClearAdditionalPreviews()
        {
            foreach (var previewObj in _additionalPreviews)
            {
                if (previewObj != null) Object.Destroy(previewObj);
            }
            _additionalPreviews.Clear();
        }

        /// Sets visibility of main preview and additional previews.
        public void SetPreviewsVisibility(bool visible, int selectionCount)
        {
            if (_preview != null)
            {
                _preview.SetActive(visible);
            }

            if (selectionCount == 0) return;

            foreach (var previewObj in _additionalPreviews)
            {
                if (previewObj != null)
                {
                    previewObj.SetActive(visible);
                }
            }
        }

        /// Sets the world position of the main preview.
        public void SetPreviewPosition(Vector3 position)
        {
            if (_preview != null)
            {
                _preview.transform.position = position;
            }
        }

        /// Rotates the main preview by the specified angle.
        public void RotatePreview(float angleDelta)
        {
            if (_preview != null)
            {
                _preview.transform.Rotate(0f, angleDelta, 0f, Space.Self);
            }
        }

        /// Rotates all additional previews by the specified angle.
        public void RotateAdditionalPreviews(float angleDelta)
        {
            foreach (var previewObj in _additionalPreviews)
            {
                if (previewObj != null)
                {
                    previewObj.transform.Rotate(0f, angleDelta, 0f, Space.Self);
                }
            }
        }

        /// Gets the current Y rotation of the main preview.
        public float GetPreviewYRotation()
        {
            return _preview != null ? _preview.transform.eulerAngles.y : 0f;
        }
    }
}
