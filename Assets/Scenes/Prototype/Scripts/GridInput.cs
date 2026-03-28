using UnityEngine;
using UnityEngine.Events;
using VTT.UI;

namespace VTT.Grid
{
    public class GridInput : MonoBehaviour
    {
        [Header("Raycast")]
        [SerializeField] private Camera    gridCamera;
        [SerializeField] private LayerMask terrainLayer;

        [Header("Events")]
        public UnityEvent<GridCell> OnCellHovered;
        public UnityEvent<GridCell> OnCellClicked;
        public UnityEvent<GridCell> OnCellRightClicked;

        private GridCell _lastHovered;

        private void Awake()
        {
            if (gridCamera == null) gridCamera = Camera.main;
        }

        private void Update()
        {
            // Never raycast / fire clicks while the cursor is over the settings panel
            if (VTTPanelUI.IsMouseOverUI) return;

            if (!Physics.Raycast(gridCamera.ScreenPointToRay(Input.mousePosition),
                                 out RaycastHit hit, Mathf.Infinity, terrainLayer))
                return;

            GridManager gm = GridManager.Instance;
            if (gm == null) return;

            Vector2Int coords = gm.WorldToGrid(hit.point);
            GridCell   cell   = gm.GetCell(coords.x, coords.y);

            if (cell != _lastHovered)
            {
                _lastHovered = cell;
                OnCellHovered?.Invoke(cell);
            }

            if (Input.GetMouseButtonDown(0)) OnCellClicked?.Invoke(cell);
            if (Input.GetMouseButtonDown(1)) OnCellRightClicked?.Invoke(cell);
        }

        public GridCell HoveredCell          => _lastHovered;
        public Vector3  HoveredWorldPosition =>
            _lastHovered != null
                ? GridManager.Instance.GridToWorld(_lastHovered.X, _lastHovered.Z)
                : Vector3.zero;
    }
}