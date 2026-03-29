using UnityEngine;
using UnityEngine.Events;

namespace VTT.Grid
{
    /// <summary>
    /// Raycasts each frame and fires UnityEvents on hover/click.
    /// Wire the events to game logic in the Inspector.
    /// UI blocking is handled by Unity's EventSystem — no code dependency needed.
    /// </summary>
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

        public GridCell HoveredCell => _lastHovered;
        public Vector3 HoveredWorldPosition =>
            _lastHovered != null
                ? GridManager.Instance.GridToWorld(_lastHovered.X, _lastHovered.Z)
                : Vector3.zero;

        private void Awake()
        {
            if (gridCamera == null) gridCamera = Camera.main;
        }

        private void Update()
        {
            if (!Physics.Raycast(gridCamera.ScreenPointToRay(Input.mousePosition),
                                 out RaycastHit hit, Mathf.Infinity, terrainLayer)) return;

            var gm = GridManager.Instance;
            if (gm == null) return;

            var coords = gm.WorldToGrid(hit.point);
            var cell   = gm.GetCell(coords.x, coords.y);

            if (cell != _lastHovered)
            {
                _lastHovered = cell;
                OnCellHovered?.Invoke(cell);
            }

            if (Input.GetMouseButtonDown(0)) OnCellClicked?.Invoke(cell);
            if (Input.GetMouseButtonDown(1)) OnCellRightClicked?.Invoke(cell);
        }
    }
}
