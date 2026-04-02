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
        [SerializeField] private float      heightOffset  = 0.6f;
        [SerializeField] private Color validColor    = new(1f, 1f, 1f, 0.3f);
        [SerializeField] private Color blockedColor  = new(1f, 0f, 0f, 0.4f);
        [SerializeField] private Color occupiedColor = new(1f, 0.5f, 0f, 0.4f);

        private Renderer _rend;
        private Material _mat;

        private void Awake()
        {
            if (highlightQuad != null)
            {
                _rend = highlightQuad.GetComponent<Renderer>();
                if (_rend != null) _mat = _rend.material; // create instance once
            }
        }

        private void OnDestroy()
        {
            if (_mat != null) Destroy(_mat);
        }

        public void OnCellHovered(GridCell cell)
        {
            if (cell == null) { Hide(); return; }
            if (highlightQuad == null) return;

            highlightQuad.SetActive(true);
            var pos = GridManager.Instance.GridToWorld(cell.X, cell.Z);
            highlightQuad.transform.position = new Vector3(pos.x, pos.y + heightOffset, pos.z);

            if (_mat != null)
                _mat.color = cell.State switch
                {
                    CellState.Blocked  => blockedColor,
                    CellState.Occupied => occupiedColor,
                    _                  => validColor
                };
        }

        public void Hide() => highlightQuad?.SetActive(false);
    }
}
