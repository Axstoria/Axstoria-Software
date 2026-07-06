using EditorShell.Presenter.View;
using Grid.Domain;
using Loxodon.Framework.Contexts;
using MapEditor.Presenter.ViewModels;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace Grid.Presenter.View
{
    public class GridInputView : MonoBehaviour
    {
        [SerializeField] private UnityEngine.Camera gridCamera;
        [SerializeField] private LayerMask terrainLayer;

        public UnityEvent<GridCell> OnCellHovered;
        public UnityEvent<GridCell> OnCellClicked;
        public UnityEvent<GridCell> OnCellRightClicked;

        public GridCell HoveredCell { get; private set; }

        private MapEditorViewModel _vm;
        private GridCell           _lastHovered;
        private IPanel             _uiPanel;
        private VisualElement      _uiRoot;

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

            if (gridCamera == null) gridCamera = UnityEngine.Camera.main;

            var uiDoc = FindFirstObjectByType<UIDocument>();
            if (uiDoc != null)
            {
                _uiRoot  = uiDoc.rootVisualElement;
                _uiPanel = _uiRoot?.panel;
            }
        }

        private void Update()
        {
            if (_vm?.Grid == null) return;

            if (UIPointerUtility.IsOverUI(_uiPanel, _uiRoot, Input.mousePosition))
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
