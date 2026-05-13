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

            Transform t = _gizmo.mainTargetRoot;

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
    }
}
