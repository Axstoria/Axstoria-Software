using UnityEngine;

namespace VTT.Grid
{
    /// <summary>
    /// Moves a highlight quad to the hovered cell and tints it by cell state.
    /// Wire OnCellHovered to GridInput.OnCellHovered in the Inspector.
    /// </summary>
    public class GridHighlighter : MonoBehaviour
    {
        [SerializeField] private GameObject highlightQuad;
        [SerializeField] private Color validColor    = new(1f, 1f, 1f, 0.3f);
        [SerializeField] private Color blockedColor  = new(1f, 0f, 0f, 0.4f);
        [SerializeField] private Color occupiedColor = new(1f, 0.5f, 0f, 0.4f);

        private Renderer _rend;

        private void Awake()
        {
            if (highlightQuad != null) _rend = highlightQuad.GetComponent<Renderer>();
        }

        public void OnCellHovered(GridCell cell)
        {
            if (cell == null || highlightQuad == null) return;

            highlightQuad.SetActive(true);
            highlightQuad.transform.position = GridManager.Instance.GridToWorld(cell.X, cell.Z);

            if (_rend != null)
                _rend.material.color = cell.State switch
                {
                    CellState.Blocked  => blockedColor,
                    CellState.Occupied => occupiedColor,
                    _                  => validColor
                };
        }

        public void Hide() => highlightQuad?.SetActive(false);
    }
}
