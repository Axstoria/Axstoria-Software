using Controler.Editor.ViewModels;
using Domain;
using Loxodon.Framework.Contexts;
using UnityEngine;

namespace Controler.Editor.Views
{
    /// <summary>
    /// Moves a highlight quad to the hovered cell and tints it by cell state.
    /// </summary>
    public class GridHighlighterView : MonoBehaviour
    {
        [SerializeField] private GameObject highlightQuad;
        [SerializeField] private float      heightOffset  = 0.01f;
        [SerializeField] private Color      validColor    = new(1f, 1f, 1f, 0.3f);
        [SerializeField] private Color      blockedColor  = new(1f, 0f, 0f, 0.4f);
        [SerializeField] private Color      occupiedColor = new(1f, 0.5f, 0f, 0.4f);

        private MapEditorViewModel _vm;
        private Material           _mat;

        private void Start()
        {
            _vm = Context.GetApplicationContext()
                         .GetContainer()
                         .Resolve<MapEditorViewModel>();

            if (_vm == null)
            {
                Debug.LogError("[GridHighlighterView] MapEditorViewModel not found.");
                enabled = false;
                return;
            }

            if (highlightQuad != null)
            {
                var rend = highlightQuad.GetComponent<Renderer>();
                if (rend != null) _mat = rend.material;
            }
        }

        private void OnDestroy()
        {
            if (_mat != null) Destroy(_mat);
        }

        // Wired to GridInputView.OnCellHovered in the Inspector
        public void OnCellHovered(GridCell cell)
        {
            if (cell == null || highlightQuad == null) { highlightQuad?.SetActive(false); return; }

            var (wx, wy, wz) = _vm.Grid.GridToWorld(cell.X, cell.Z);
            highlightQuad.SetActive(true);
            highlightQuad.transform.position = new Vector3(wx, wy + heightOffset, wz);

            if (_mat != null)
                _mat.color = cell.State switch
                {
                    CellState.Blocked  => blockedColor,
                    CellState.Occupied => occupiedColor,
                    _                  => validColor
                };
        }
    }
}
