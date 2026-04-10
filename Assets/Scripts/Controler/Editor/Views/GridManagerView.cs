using Controler.Editor.ViewModels;
using Domain;
using Loxodon.Framework.Contexts;
using UnityEngine;
using UnityEngine.Events;
using DomainGrid = Domain.Grid;

namespace Controler.Editor.Views
{
    /// <summary>
    /// Unity-side adapter for Domain.Grid.
    /// Wraps tuple-based coordinate methods into Unity Vector3 helpers,
    /// and relays Grid.OnCellChanged as a UnityEvent for Inspector wiring.
    /// Other Views should use these helpers instead of converting Domain tuples manually.
    /// </summary>
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

        // ── Coordinate helpers ────────────────────────────────────────────────

        /// <summary>Snaps a world position to the centre of its grid cell.</summary>
        public Vector3 SnapToGrid(Vector3 worldPos)
        {
            if (_grid == null) return worldPos;
            var (gx, gz)     = _grid.WorldToGrid(worldPos.x, worldPos.z);
            var (wx, wy, wz) = _grid.GridToWorld(gx, gz);
            return new Vector3(wx, wy, wz);
        }

        /// <summary>Returns the world-space centre of a grid cell as a Unity Vector3.</summary>
        public Vector3 GridToWorld(int x, int z)
        {
            if (_grid == null) return Vector3.zero;
            var (wx, wy, wz) = _grid.GridToWorld(x, z);
            return new Vector3(wx, wy, wz);
        }

        /// <summary>Returns the grid coordinates for a world position.</summary>
        public (int x, int z) WorldToGrid(Vector3 worldPos)
        {
            if (_grid == null) return (0, 0);
            return _grid.WorldToGrid(worldPos.x, worldPos.z);
        }

        /// <summary>Returns the cell at a world position, or null if not loaded.</summary>
        public GridCell GetCellAt(Vector3 worldPos)
        {
            if (_grid == null) return null;
            var (gx, gz) = _grid.WorldToGrid(worldPos.x, worldPos.z);
            return _grid.GetCellSafe(gx, gz);
        }
    }
}
