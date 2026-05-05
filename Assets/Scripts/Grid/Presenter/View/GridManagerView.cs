using Grid.Domain;
using Loxodon.Framework.Contexts;
using MapEditor.Presenter.ViewModels;
using UnityEngine;
using UnityEngine.Events;
using DomainGrid = Grid.Domain.Grid;

namespace Grid.Presenter.View
{
    public class GridManagerView : MonoBehaviour
    {
        public UnityEvent<GridCell> OnCellChanged;

        private MapEditorViewModel _vm;
        private DomainGrid         _grid;

        private void Start()
        {
            _vm = Context.GetApplicationContext()
                         .GetContainer()
                         .Resolve<MapEditorViewModel>();

            if (_vm == null)
            {
                Debug.LogError("[GridManagerView] MapEditorViewModel not found.");
                enabled = false;
                return;
            }

            _grid = _vm.Grid;

            if (_grid != null)
                _grid.OnCellChanged += cell => OnCellChanged?.Invoke(cell);
        }

        public Vector3 SnapToGrid(Vector3 worldPos)
        {
            if (_grid == null) return worldPos;
            var (gx, gz)     = _grid.WorldToGrid(worldPos.x, worldPos.z);
            var (wx, wy, wz) = _grid.GridToWorld(gx, gz);
            return new Vector3(wx, wy, wz);
        }

        public Vector3 GridToWorld(int x, int z)
        {
            if (_grid == null) return Vector3.zero;
            var (wx, wy, wz) = _grid.GridToWorld(x, z);
            return new Vector3(wx, wy, wz);
        }

        public (int x, int z) WorldToGrid(Vector3 worldPos)
        {
            if (_grid == null) return (0, 0);
            return _grid.WorldToGrid(worldPos.x, worldPos.z);
        }

        public GridCell GetCellAt(Vector3 worldPos)
        {
            if (_grid == null) return null;
            var (gx, gz) = _grid.WorldToGrid(worldPos.x, worldPos.z);
            return _grid.GetCellSafe(gx, gz);
        }
    }
}
