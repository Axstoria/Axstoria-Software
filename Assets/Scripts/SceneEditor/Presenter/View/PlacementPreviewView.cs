using System;
using System.Collections.Generic;
using Grid.Domain;
using Loxodon.Framework.Contexts;
using MapEditor.Presenter.ViewModels;
using SceneEditor.Domain;
using UnityEngine;

namespace SceneEditor.Presenter.View
{
    public class PlacementPreviewView : MonoBehaviour
    {
        [SerializeField] private Material validMaterial;
        [SerializeField] private Material invalidMaterial;

        private MapEditorViewModel _vm;
        private GameObject         _previewInstance;
        private SceneObject        _pendingObject;
        private List<GridCoord>    _cachedFootprint;
        private Renderer[]         _cachedRenderers;
        private EventHandler       _onPlacementModeChanged;

        private void Start()
        {
            _vm = Context.GetApplicationContext()
                         .GetContainer()
                         .Resolve<MapEditorViewModel>();

            if (_vm == null)
            {
                Debug.LogError("[PlacementPreviewView] MapEditorViewModel not found.");
                enabled = false;
                return;
            }

            _onPlacementModeChanged = (_, __) => { if (!_vm.IsPlacementMode.Value) CancelPreview(); };
            _vm.IsPlacementMode.ValueChanged += _onPlacementModeChanged;
        }

        private void OnDestroy()
        {
            if (_vm != null)
                _vm.IsPlacementMode.ValueChanged -= _onPlacementModeChanged;

            if (_previewInstance != null)
                Destroy(_previewInstance);
        }

        public void BeginPlacement(GameObject prefab, SceneObject domainObject)
        {
            CancelPreview();
            _pendingObject   = domainObject;
            _previewInstance = Instantiate(prefab);
            _cachedRenderers = _previewInstance.GetComponentsInChildren<Renderer>();
            _cachedFootprint = ComputeFootprint();
            SetPreviewMaterial(invalidMaterial);
            _vm.IsPlacementMode.Value = true;
        }

        public void OnCellHovered(GridCell cell)
        {
            if (_previewInstance == null || cell == null) return;

            var origin       = new GridCoord(cell.X, cell.Z);
            var canPlace     = _vm.PlaceObject.CanPlace(_cachedFootprint, origin);
            var (wx, wy, wz) = _vm.Grid.GridToWorld(cell.X, cell.Z);
            _previewInstance.transform.position = new Vector3(wx, wy, wz);
            SetPreviewMaterial(canPlace ? validMaterial : invalidMaterial);
        }

        public void OnCellClicked(GridCell cell)
        {
            if (_previewInstance == null || _pendingObject == null || cell == null) return;

            _vm.PlaceObject.Execute(_pendingObject, new GridCoord(cell.X, cell.Z), _cachedFootprint);
            CancelPreview();
        }

        private void CancelPreview()
        {
            if (_previewInstance != null) Destroy(_previewInstance);
            _previewInstance = null;
            _pendingObject   = null;
            _cachedFootprint = null;
            _cachedRenderers = null;
            if (_vm != null) _vm.IsPlacementMode.Value = false;
        }

        private List<GridCoord> ComputeFootprint()
        {
            float cellSize = _vm.Grid?.CellSize ?? 1f;
            if (_cachedRenderers == null || _cachedRenderers.Length == 0)
                return new List<GridCoord> { GridCoord.Zero };

            var bounds = _cachedRenderers[0].bounds;
            for (int i = 1; i < _cachedRenderers.Length; i++) bounds.Encapsulate(_cachedRenderers[i].bounds);

            int cellsX = Mathf.Max(1, Mathf.RoundToInt(bounds.size.x / cellSize));
            int cellsZ = Mathf.Max(1, Mathf.RoundToInt(bounds.size.z / cellSize));
            int offX   = cellsX / 2;
            int offZ   = cellsZ / 2;

            var footprint = new List<GridCoord>(cellsX * cellsZ);
            for (int x = 0; x < cellsX; x++)
            for (int z = 0; z < cellsZ; z++)
                footprint.Add(new GridCoord(x - offX, z - offZ));

            return footprint;
        }

        private void SetPreviewMaterial(Material mat)
        {
            if (mat == null || _cachedRenderers == null) return;
            foreach (var r in _cachedRenderers)
                r.material = mat;
        }
    }
}
