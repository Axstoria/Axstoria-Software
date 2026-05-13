using Loxodon.Framework.Contexts;
using MapEditor.Presenter.ViewModels;
using RuntimeGizmos;
using SceneEditor.Domain;
using UnityEngine;

namespace MapEditor.Presenter.View
{
    /// Adapts RuntimeGizmo's TransformGizmo to the domain architecture.
    /// Must live on the same GameObject as TransformGizmo (the main camera).
    [RequireComponent(typeof(TransformGizmo))]
    public class TransformGizmoView : MonoBehaviour
    {
        private TransformGizmo     _gizmo;
        private MapEditorViewModel _vm;
        private SceneObject        _domainObj;

        public bool SnapToGridEnabled    { get; set; }
        public bool SnapToTerrainEnabled { get; set; }

        private void Start()
        {
            _gizmo = GetComponent<TransformGizmo>();
            _gizmo.useExternalSelection  = true;
            _gizmo.onTransformCompleted += OnTransformCompleted;

            _vm = Context.GetApplicationContext().GetContainer().Resolve<MapEditorViewModel>();
            if (_vm == null)
            {
                Debug.LogError("[TransformGizmoView] MapEditorViewModel not found.");
                enabled = false;
            }
        }

        private void OnDestroy()
        {
            if (_gizmo != null)
                _gizmo.onTransformCompleted -= OnTransformCompleted;
        }

        // ── Public API (called by SideBarController and scene click handlers) ─

        public bool IsInteractingWithGizmo
            => _gizmo != null && (_gizmo.isTransforming || _gizmo.translatingAxis != Axis.None);

        public void SetTransformType(TransformType type)
        {
            if (_gizmo != null) _gizmo.transformType = type;
        }

        public void Select(GameObject go, SceneObject domainObj)
        {
            _domainObj = domainObj;
            _gizmo.ClearTargets(addCommand: false);
            _gizmo.AddTarget(go.transform, addCommand: false);
        }

        public void Deselect()
        {
            _domainObj = null;
            _gizmo.ClearTargets(addCommand: false);
        }

        // ── Delete key ────────────────────────────────────────────────────────

        private void Update()
        {
            if (_domainObj == null || _vm == null) return;

            if (Input.GetKeyDown(KeyCode.Delete))
            {
                _vm.DeleteObject.Execute(_domainObj);
                Deselect();
            }
        }

        // ── Transform completion → domain ─────────────────────────────────────

        private void OnTransformCompleted()
        {
            if (_domainObj == null || _vm == null || _gizmo.mainTargetRoot == null) return;

            Transform t   = _gizmo.mainTargetRoot;
            Vector3   pos = t.position;

            if (SnapToGridEnabled)
                pos = SnapPositionToGrid(pos);

            if (SnapToTerrainEnabled)
                pos = SnapPositionToTerrain(pos, t);

            if (SnapToGridEnabled || SnapToTerrainEnabled)
                t.position = pos;

            string label = _gizmo.transformType switch
            {
                TransformType.Move   => $"Move {_domainObj.DisplayName}",
                TransformType.Rotate => $"Rotate {_domainObj.DisplayName}",
                TransformType.Scale  => $"Scale {_domainObj.DisplayName}",
                _                    => "Transform"
            };

            _vm.TransformObject.Execute(_domainObj, new TransformModel
            {
                Position = t.position,
                Rotation = t.rotation,
                Scale    = t.localScale
            }, label);
        }

        private Vector3 SnapPositionToGrid(Vector3 pos)
        {
            var grid = _vm.Grid;
            if (grid == null) return pos;
            (int gx, int gz)             = grid.WorldToGrid(pos.x, pos.z);
            (float wx, float wy, float wz) = grid.GridToWorld(gx, gz);
            return new Vector3(wx, wy, wz);
        }

        private static Vector3 SnapPositionToTerrain(Vector3 pos, Transform target)
        {
            var origin = new Vector3(pos.x, pos.y + 500f, pos.z);
            RaycastHit[] hits = Physics.RaycastAll(origin, Vector3.down, Mathf.Infinity);
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
            foreach (var hit in hits)
            {
                if (hit.transform == target || hit.transform.IsChildOf(target)) continue;
                return new Vector3(pos.x, hit.point.y, pos.z);
            }
            return pos;
        }
    }
}
