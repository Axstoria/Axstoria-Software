using Controler.Editor.ViewModels;
using Domain;
using Loxodon.Framework.Contexts;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Controler.Editor.Views
{
    /// <summary>
    /// Raycasts each frame to detect which grid cell the cursor is over.
    /// Fires events on hover and click — other Views subscribe to react.
    /// No placement logic lives here, only input detection and forwarding.
    /// </summary>
    public class GridInputView : MonoBehaviour
    {
        [SerializeField] private Camera    gridCamera;
        [SerializeField] private LayerMask terrainLayer;

        public UnityEvent<GridCell> OnCellHovered;
        public UnityEvent<GridCell> OnCellClicked;
        public UnityEvent<GridCell> OnCellRightClicked;

        public GridCell HoveredCell { get; private set; }

        private MapEditorViewModel _vm;
        private GridCell           _lastHovered;

        private void Start()
        {
            _vm = Context.GetApplicationContext()
                         .GetContainer()
                         .Resolve<MapEditorViewModel>();

            if (_vm == null)
            {
                Debug.LogError("[GridInputView] MapEditorViewModel not found.");
                enabled = false;
                return;
            }

            if (gridCamera == null) gridCamera = Camera.main;
        }

        private void Update()
        {
            if (_vm?.Grid == null) return;

            // Block input when cursor is over UI
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                if (_lastHovered != null) { _lastHovered = null; OnCellHovered?.Invoke(null); }
                return;
            }

            if (!Physics.Raycast(gridCamera.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, Mathf.Infinity, terrainLayer))
            {
                if (_lastHovered != null) { _lastHovered = null; OnCellHovered?.Invoke(null); }
                return;
            }

            var (gx, gz) = _vm.Grid.WorldToGrid(hit.point.x, hit.point.z);
            var cell     = _vm.Grid.GetCell(gx, gz);

            if (cell != _lastHovered)
            {
                _lastHovered = cell;
                HoveredCell  = cell;
                OnCellHovered?.Invoke(cell);
            }

            if (Input.GetMouseButtonDown(0)) OnCellClicked?.Invoke(cell);
            if (Input.GetMouseButtonDown(1)) OnCellRightClicked?.Invoke(cell);
        }
    }
}
