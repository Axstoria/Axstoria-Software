using MapEditor.Presenter.View;
using SceneEditor.Domain;
using UnityEngine;
using UnityEngine.UIElements;

namespace EditorShell.Presenter.View
{
    public class SnapToolbarController : MonoBehaviour
    {
        private const string SelectedClass = "view-btn--selected";

        private TransformGizmoView _gizmo;
        private VisualElement _snapBar;
        private Button _gridBtn;
        private Button _terrainBtn;

        public void Init(VisualElement root)
        {
            _gizmo      = FindFirstObjectByType<TransformGizmoView>();
            _snapBar    = root.Q<VisualElement>("snap-bar");
            _gridBtn    = root.Q<Button>("btn-snap-grid");
            _terrainBtn = root.Q<Button>("btn-snap-terrain");

            if (_gridBtn != null)    _gridBtn.clicked    += ToggleGrid;
            if (_terrainBtn != null) _terrainBtn.clicked += ToggleTerrain;

            if (_snapBar != null) _snapBar.style.display = DisplayStyle.None;
            if (_gizmo != null) _gizmo.OnSelectionChanged += OnSelectionChanged;
        }

        private void OnDestroy()
        {
            if (_gizmo != null) _gizmo.OnSelectionChanged -= OnSelectionChanged;
        }

        private void OnSelectionChanged(SceneObject model)
        {
            if (_snapBar == null) return;
            _snapBar.style.display = model != null ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void ToggleGrid()
        {
            bool enabled = !_gridBtn.ClassListContains(SelectedClass);
            SetState(_gridBtn, enabled);
            if (_gizmo != null) _gizmo.SnapToGridEnabled = enabled;
        }

        private void ToggleTerrain()
        {
            bool enabled = !_terrainBtn.ClassListContains(SelectedClass);
            SetState(_terrainBtn, enabled);
            if (_gizmo != null) _gizmo.SnapToTerrainEnabled = enabled;
        }

        private static void SetState(Button btn, bool enabled)
        {
            if (enabled) btn.AddToClassList(SelectedClass);
            else         btn.RemoveFromClassList(SelectedClass);
        }
    }
}
