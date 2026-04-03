using System.Collections.Generic;
using Controler.Editor.ViewModels;
using Domain;
using Loxodon.Framework.Contexts;
using UnityEngine;

namespace Controler.Editor.Views
{
    /// <summary>
    /// Manages the ghost preview object during placement mode.
    /// Observes IsPlacementMode on the ViewModel and GridInputView hover events.
    /// When the user clicks, computes the footprint from renderer bounds
    /// and calls PlaceObjectUseCase — the only View that touches placement logic.
    /// </summary>
    public class PlacementPreviewView : MonoBehaviour
    {
        [SerializeField] private Material validMaterial;
        [SerializeField] private Material invalidMaterial;

        private MapEditorViewModel _vm;
        private GameObject         _previewInstance;
        private SceneObject        _pendingObject;

        private void Start()
        {
            _vm = Context.GetApplicationContext()
                         .GetContainer()
                         .Resolve<MapEditorViewModel>();

            _vm.IsPlacementMode.ValueChanged += (_, __) =>
            {
                if (!_vm.IsPlacementMode.Value) CancelPreview();
            };
        }

        // Called by UI when user selects an object to place
        public void BeginPlacement(GameObject prefab, SceneObject domainObject)
        {
            CancelPreview();
            _pendingObject   = domainObject;
            _previewInstance = Instantiate(prefab);
            SetPreviewMaterial(invalidMaterial);
            _vm.IsPlacementMode.Value = true;
        }

        // Wired to GridInputView.OnCellHovered in the Inspector
        public void OnCellHovered(GridCell cell)
        {
            if (_previewInstance == null || cell == null) return;

            var footprint                = ComputeFootprint(_previewInstance);
            var origin                   = new GridCoord(cell.X, cell.Z);
            var canPlace                 = _vm.PlaceObject.CanPlace(footprint, origin);
            var (wx, wy, wz)             = _vm.Grid.GridToWorld(cell.X, cell.Z);
            _previewInstance.transform.position = new Vector3(wx, wy, wz);
            SetPreviewMaterial(canPlace ? validMaterial : invalidMaterial);
        }

        // Wired to GridInputView.OnCellClicked in the Inspector
        public void OnCellClicked(GridCell cell)
        {
            if (_previewInstance == null || _pendingObject == null || cell == null) return;

            var origin    = new GridCoord(cell.X, cell.Z);
            var footprint = ComputeFootprint(_previewInstance);

            _vm.PlaceObject.Execute(_pendingObject, origin, footprint);
            CancelPreview();
        }

        private void CancelPreview()
        {
            if (_previewInstance != null) Destroy(_previewInstance);
            _previewInstance          = null;
            _pendingObject            = null;
            _vm.IsPlacementMode.Value = false;
        }

        // Computes grid footprint from the preview object's renderer bounds
        private List<GridCoord> ComputeFootprint(GameObject obj)
        {
            float cellSize = _vm.Grid?.CellSize ?? 1f;
            var renderers  = obj.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return new List<GridCoord> { GridCoord.Zero };

            var bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);

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
            if (mat == null || _previewInstance == null) return;
            foreach (var r in _previewInstance.GetComponentsInChildren<Renderer>())
                r.material = mat;
        }
    }
}
