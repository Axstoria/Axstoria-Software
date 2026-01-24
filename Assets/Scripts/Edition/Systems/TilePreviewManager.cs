using System.Collections.Generic;
using HexGrid.Models;
using UnityEngine;

namespace HexGrid.Systems
{
    /// Manages preview GameObjects for tile placement and multi-selection.
    public class TilePreviewManager
    {
        private readonly GameObject _preview;
        /// Additional previews for multi-tile selection movement.
        private readonly List<GameObject> _additionalPreviews = new();
        private readonly HexGridManager _gridManager;

        private Material _previewOriginalMaterial;
        /// Hides previews after keyboard movement until mouse moves again.
        private bool _hidePreviewsUntilMouseMove;

        public bool HidePreviewsUntilMouseMove
        {
            get => _hidePreviewsUntilMouseMove;
            set => _hidePreviewsUntilMouseMove = value;
        }

        public TilePreviewManager(GameObject preview, HexGridManager gridManager)
        {
            _preview = preview;
            _gridManager = gridManager;
        }

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

        public void ClearAdditionalPreviews()
        {
            foreach (var previewObj in _additionalPreviews)
            {
                if (previewObj != null) Object.Destroy(previewObj);
            }
            _additionalPreviews.Clear();
        }

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

        public void SetPreviewPosition(Vector3 position)
        {
            if (_preview != null)
            {
                _preview.transform.position = position;
            }
        }

        public void RotatePreview(float angleDelta)
        {
            if (_preview != null)
            {
                _preview.transform.Rotate(0f, angleDelta, 0f, Space.Self);
            }
        }

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

        public float GetPreviewYRotation()
        {
            return _preview != null ? _preview.transform.eulerAngles.y : 0f;
        }
    }
}
