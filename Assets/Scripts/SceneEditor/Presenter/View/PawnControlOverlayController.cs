using System.Collections;
using System.Collections.Generic;
using Grid.Domain;
using Grid.Presenter.View;
using Loxodon.Framework.Contexts;
using MapEditor.Presenter.View;
using MapEditor.Presenter.ViewModels;
using SceneEditor.Domain;
using UnityEngine;
using UnityEngine.UIElements;

namespace SceneEditor.Presenter.View
{
    public class PawnControlOverlayController : MonoBehaviour
    {
        private const float MoveDuration = 0.25f;
        private const float RotateDuration = 0.2f;
        private const float PanelHeightOffset = 0.15f;

        private TransformGizmoView _gizmo;
        private SceneObjectSpawnerView _spawner;
        private PlacementPreviewView _placementPreview;
        private GridInputView _gridInput;
        private MapEditorViewModel _vm;

        private VisualElement _panel;
        private Button _btnRotateLeft;
        private Button _btnRotateRight;
        private IPanel _uiPanel;

        private SceneObject _activePawn;
        private GameObject _ghost;
        private Renderer[] _ghostRenderers;
        private bool _isAnimating;

        public void Init(VisualElement root)
        {
            _uiPanel = root.panel;

            VisualTreeAsset asset = Resources.Load<VisualTreeAsset>("Viewport/PawnControlOverlay");
            VisualElement instance = asset.Instantiate();
            root.Add(instance);

            _panel = instance.Q<VisualElement>("pawn-control-overlay");
            _btnRotateLeft = instance.Q<Button>("btn-rotate-left");
            _btnRotateRight = instance.Q<Button>("btn-rotate-right");
            _panel.style.display = DisplayStyle.None;

            _btnRotateLeft.clicked += () => RotatePawn(-90f);
            _btnRotateRight.clicked += () => RotatePawn(90f);

            _gizmo = FindFirstObjectByType<TransformGizmoView>();
            _spawner = FindFirstObjectByType<SceneObjectSpawnerView>();
            _placementPreview = FindFirstObjectByType<PlacementPreviewView>();
            _gridInput = FindFirstObjectByType<GridInputView>();

            if (_gizmo == null || _spawner == null)
            {
                Debug.LogWarning("[PawnControlOverlayController] TransformGizmoView/SceneObjectSpawnerView not found.");
                enabled = false;
                return;
            }

            _vm = Context.GetApplicationContext().GetContainer().Resolve<MapEditorViewModel>();
            if (_vm == null)
            {
                Debug.LogWarning("[PawnControlOverlayController] MapEditorViewModel not registered.");
                enabled = false;
                return;
            }

            _gizmo.OnSelectionChanged += OnSelectionChanged;

            if (_gridInput != null)
            {
                _gridInput.OnCellHovered.AddListener(OnCellHovered);
                _gridInput.OnCellClicked.AddListener(OnCellClicked);
            }
            else
            {
                Debug.LogWarning("[PawnControlOverlayController] GridInputView not found; pawn grid movement disabled.");
            }
        }

        private void OnDestroy()
        {
            if (_gizmo != null) _gizmo.OnSelectionChanged -= OnSelectionChanged;
            if (_gridInput != null)
            {
                _gridInput.OnCellHovered.RemoveListener(OnCellHovered);
                _gridInput.OnCellClicked.RemoveListener(OnCellClicked);
            }
            DestroyGhost();
        }

        private void Update()
        {
            if (_gizmo == null || _vm == null) return;

            if (!IsActive())
            {
                if (_panel.style.display != DisplayStyle.None)
                    _panel.style.display = DisplayStyle.None;
                DestroyGhost();
                return;
            }

            if (!_isAnimating)
            {
                _panel.style.display = DisplayStyle.Flex;
                PositionPanel();
            }
        }

        private bool IsActive()
            => _gizmo.IsSelectModeActive && _activePawn != null && _activePawn.IsPawn && !_isAnimating;

        private void OnSelectionChanged(SceneObject domainObj)
        {
            _activePawn = domainObj;
            DestroyGhost();
        }

        private void PositionPanel()
        {
            if (_activePawn == null || !_spawner.TryGetGameObject(_activePawn.Id, out GameObject go))
            {
                _panel.style.display = DisplayStyle.None;
                return;
            }

            var cam = UnityEngine.Camera.main;
            if (cam == null) return;

            Vector3 screen = cam.WorldToScreenPoint(go.transform.position + Vector3.up * PanelHeightOffset);
            if (screen.z < 0)
            {
                _panel.style.display = DisplayStyle.None;
                return;
            }

            Vector2 panelPos = RuntimePanelUtils.ScreenToPanel(
                _uiPanel, new Vector2(screen.x, Screen.height - screen.y));
            _panel.style.left = panelPos.x - _panel.resolvedStyle.width * 0.5f;
            _panel.style.top  = panelPos.y - _panel.resolvedStyle.height * 0.5f;
        }

        // ── Movement ──────────────────────────────────────────────────────────

        private void OnCellHovered(GridCell cell)
        {
            if (!IsActive() || cell == null || _vm.Grid == null)
            {
                HideGhost();
                return;
            }

            EnsureGhost();
            if (_ghost == null) return;

            var (wx, wy, wz) = _vm.Grid.GridToWorld(cell.X, cell.Z);
            _ghost.transform.position = new Vector3(wx, wy, wz);
            _ghost.SetActive(true);

            SetGhostMaterial(CanMoveTo(cell));
        }

        private void OnCellClicked(GridCell cell)
        {
            if (!IsActive() || cell == null || _vm.Grid == null) return;
            if (!CanMoveTo(cell)) return;
            if (!_vm.Permissions.CanInteract(_vm.Session.CurrentPlayer, _activePawn)) return;

            var origin = new GridCoord(cell.X, cell.Z);
            if (_vm.Grid.TryGetPlacement(_activePawn.Id, out GridCoord currentOrigin, out _)
                && currentOrigin.X == origin.X && currentOrigin.Z == origin.Z)
                return;

            if (!_spawner.TryGetGameObject(_activePawn.Id, out GameObject go)) return;

            var (wx, wy, wz) = _vm.Grid.GridToWorld(cell.X, cell.Z);
            HideGhost();
            StartCoroutine(AnimateMove(go, new Vector3(wx, wy, wz), origin));
        }

        private bool CanMoveTo(GridCell cell)
        {
            var origin = new GridCoord(cell.X, cell.Z);

            if (_vm.Grid.TryGetPlacement(_activePawn.Id, out GridCoord currentOrigin, out _)
                && currentOrigin.X == origin.X && currentOrigin.Z == origin.Z)
                return true;

            return _vm.Grid.CanPlace(SingleCellFootprint, origin);
        }

        private static readonly List<GridCoord> SingleCellFootprint = new() { GridCoord.Zero };

        private IEnumerator AnimateMove(GameObject go, Vector3 destination, GridCoord newOrigin)
        {
            _isAnimating = true;
            Vector3 start = go.transform.position;
            float t = 0f;
            while (t < MoveDuration)
            {
                t += Time.deltaTime;
                go.transform.position = Vector3.Lerp(start, destination, Mathf.Clamp01(t / MoveDuration));
                yield return null;
            }
            go.transform.position = destination;

            _vm.Grid.RemoveOccupant(_activePawn.Id);
            _vm.Grid.PlaceOccupant(_activePawn.Id, SingleCellFootprint, newOrigin);

            _vm.TransformObject.Execute(_activePawn, new TransformModel
            {
                Position = go.transform.position,
                Rotation = go.transform.rotation,
                Scale    = go.transform.localScale
            }, "Move pawn");

            _isAnimating = false;
        }

        // ── Rotation ──────────────────────────────────────────────────────────

        private void RotatePawn(float degrees)
        {
            if (!IsActive()) return;
            if (!_vm.Permissions.CanInteract(_vm.Session.CurrentPlayer, _activePawn)) return;
            if (!_spawner.TryGetGameObject(_activePawn.Id, out GameObject go)) return;

            StartCoroutine(AnimateRotate(go, degrees));
        }

        private IEnumerator AnimateRotate(GameObject go, float degrees)
        {
            _isAnimating = true;
            Quaternion start = go.transform.rotation;
            Quaternion end   = start * Quaternion.Euler(0f, degrees, 0f);
            float t = 0f;
            while (t < RotateDuration)
            {
                t += Time.deltaTime;
                go.transform.rotation = Quaternion.Slerp(start, end, Mathf.Clamp01(t / RotateDuration));
                yield return null;
            }
            go.transform.rotation = end;

            _vm.TransformObject.Execute(_activePawn, new TransformModel
            {
                Position = go.transform.position,
                Rotation = go.transform.rotation,
                Scale    = go.transform.localScale
            }, "Rotate pawn");

            _isAnimating = false;
        }

        // ── Ghost preview ─────────────────────────────────────────────────────

        private void EnsureGhost()
        {
            if (_ghost != null) return;
            if (_activePawn == null || !_spawner.TryGetGameObject(_activePawn.Id, out GameObject realGo)) return;

            _ghost = Instantiate(realGo);
            _ghost.name = "PawnMoveGhost";

            foreach (var view in _ghost.GetComponentsInChildren<SceneObjectView>(true))
                Destroy(view);
            foreach (var col in _ghost.GetComponentsInChildren<Collider>(true))
                Destroy(col);

            _ghostRenderers = _ghost.GetComponentsInChildren<Renderer>(true);
            _ghost.SetActive(false);
        }

        private void SetGhostMaterial(bool valid)
        {
            if (_ghostRenderers == null || _placementPreview == null) return;

            Material mat = valid ? _placementPreview.ValidMaterial : _placementPreview.InvalidMaterial;
            if (mat == null) return;

            foreach (Renderer r in _ghostRenderers)
                r.material = mat;
        }

        private void HideGhost()
        {
            if (_ghost != null) _ghost.SetActive(false);
        }

        private void DestroyGhost()
        {
            if (_ghost != null) Destroy(_ghost);
            _ghost = null;
            _ghostRenderers = null;
        }
    }
}
