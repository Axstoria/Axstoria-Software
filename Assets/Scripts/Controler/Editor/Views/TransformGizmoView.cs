using Controler.Editor.ViewModels;
using Domain;
using Loxodon.Framework.Contexts;
using UnityEngine;
using DomainVec3 = Domain.Math.Vector3;
using DomainQuat = Domain.Math.Quaternion;

namespace Controler.Editor.Views
{
    /// <summary>
    /// Gizmo for translating, rotating, and scaling the selected object.
    /// Observes the selected SceneObject and displays handles around it.
    /// </summary>
    /// NEEDS REWORK
    public class TransformGizmoView : MonoBehaviour
    {
        // ── Enums ─────────────────────────────────────────────────────────────

        private enum GizmoMode { Translate, Rotate, Scale }

        private enum Handle
        {
            None,
            AxisX, AxisY, AxisZ,
            PlaneXY, PlaneYZ, PlaneXZ,
            RotateX, RotateY, RotateZ
        }

        // ── Colors ────────────────────────────────────────────────────────────

        private static readonly Color ColX     = new(0.95f, 0.20f, 0.20f);
        private static readonly Color ColY     = new(0.20f, 0.95f, 0.20f);
        private static readonly Color ColZ     = new(0.20f, 0.45f, 1.00f);
        private static readonly Color ColXY    = new(1.00f, 1.00f, 0.20f, 0.55f);
        private static readonly Color ColYZ    = new(0.20f, 1.00f, 1.00f, 0.55f);
        private static readonly Color ColXZ    = new(1.00f, 0.20f, 1.00f, 0.55f);
        private static readonly Color ColHover = new(1.00f, 1.00f, 1.00f, 0.95f);

        // ── Sizing (relative to gizmo scale) ──────────────────────────────────

        private const float ShaftLength       = 1.00f;
        private const float ShaftRadius       = 0.04f;
        private const float ArrowLength       = 0.28f;
        private const float ArrowRadius       = 0.09f;
        private const float CubeTipRadius     = 0.10f;
        private const float PlaneOffset       = 0.22f;
        private const float PlaneSize         = 0.22f;
        private const float RotateRadius      = 1.10f;
        private const float HoverAxisTol      = 0.14f;
        private const float HoverRingTol      = 0.18f;
        private const float RotateSensitivity = 0.50f;
        private const float ScaleSensitivity  = 0.005f;
        private const float GizmoScaleFactor  = 0.15f;

        // ── State ─────────────────────────────────────────────────────────────

        private MapEditorViewModel _vm;
        private Camera             _cam;

        private GameObject  _target;
        private SceneObject _domainObj;

        private GizmoMode _mode    = GizmoMode.Translate;
        private bool      _isLocal = false;

        private Handle _hovered = Handle.None;
        private Handle _active  = Handle.None;
        private bool   _isDragging;

        // ── Drag state ────────────────────────────────────────────────────────

        // Axis drag
        private Vector3 _dragAxis;
        private Vector3 _dragStartWorldPoint;

        // Plane drag
        private Vector3 _dragPlaneNormal;

        // Rotate
        private float      _dragStartMouseX;
        private Quaternion _dragStartRot;

        // Scale plane
        private Vector2 _dragStartMousePos;
        private int     _scaleAxisA, _scaleAxisB;

        // Object state at drag start (for live reset and undo)
        private Vector3    _positionBefore;
        private Quaternion _rotationBefore;
        private Vector3    _scaleBefore;
        private Vector3    _dragStartPos;
        private Vector3    _dragStartScale;

        // ── Unity lifecycle ───────────────────────────────────────────────────

        private void Start()
        {
            _vm  = Context.GetApplicationContext().GetContainer().Resolve<MapEditorViewModel>();
            _cam = Camera.main;

            if (_vm == null)
            {
                Debug.LogError("[TransformGizmoView] MapEditorViewModel not found.");
                enabled = false;
            }
        }

        private void Update()
        {
            if (_target == null) return;
            if (_cam == null) _cam = Camera.main;

            HandleModeKeys();

            float s = GizmoScale();

            if (_isDragging)
            {
                UpdateDrag();
                if (Input.GetMouseButtonUp(0)) EndDrag();
            }
            else
            {
                _hovered = DetectHover(s);
                if (Input.GetMouseButtonDown(0))
                {
                    if (_hovered != Handle.None) StartDrag(s);
                    else                         Deselect();
                }
            }
        }

        private void OnRenderObject()
        {
            if (_target == null || Camera.current == null) return;

            Vector3 o = _target.transform.position;
            float   s = GizmoScale();

            switch (_mode)
            {
                case GizmoMode.Translate: DrawTranslateHandles(o, s); break;
                case GizmoMode.Rotate:    DrawRotateHandles(o, s);    break;
                case GizmoMode.Scale:     DrawScaleHandles(o, s);     break;
            }
        }

        // ── Public API ────────────────────────────────────────────────────────

        public void Select(GameObject go, SceneObject domainObj)
        {
            _target    = go;
            _domainObj = domainObj;
        }

        public void Deselect()
        {
            _target     = null;
            _domainObj  = null;
            _isDragging = false;
            _active     = Handle.None;
        }

        // ── Keyboard shortcuts ────────────────────────────────────────────────

        private void HandleModeKeys()
        {
            if (Input.GetKeyDown(KeyCode.W)) _mode    = GizmoMode.Translate;
            if (Input.GetKeyDown(KeyCode.E)) _mode    = GizmoMode.Rotate;
            if (Input.GetKeyDown(KeyCode.R)) _mode    = GizmoMode.Scale;
            if (Input.GetKeyDown(KeyCode.Q)) _isLocal = !_isLocal;

            if (Input.GetKeyDown(KeyCode.Delete) && _domainObj != null)
            {
                _vm.DeleteObject.Execute(_domainObj);
                Deselect();
            }
        }

        // ── Hover detection ───────────────────────────────────────────────────

        private Handle DetectHover(float s)
        {
            Vector3 o = _target.transform.position;

            if (_mode == GizmoMode.Rotate)
            {
                if (HoverRing(o, WorldAxisX, s)) return Handle.RotateX;
                if (HoverRing(o, WorldAxisY, s)) return Handle.RotateY;
                if (HoverRing(o, WorldAxisZ, s)) return Handle.RotateZ;
                return Handle.None;
            }

            // Plane handles first — they sit inside the axis shafts
            if (HoverPlane(o, WorldAxisX, WorldAxisY, s)) return Handle.PlaneXY;
            if (HoverPlane(o, WorldAxisY, WorldAxisZ, s)) return Handle.PlaneYZ;
            if (HoverPlane(o, WorldAxisX, WorldAxisZ, s)) return Handle.PlaneXZ;

            if (HoverAxis(o, WorldAxisX, s)) return Handle.AxisX;
            if (HoverAxis(o, WorldAxisY, s)) return Handle.AxisY;
            if (HoverAxis(o, WorldAxisZ, s)) return Handle.AxisZ;

            return Handle.None;
        }

        private bool HoverAxis(Vector3 origin, Vector3 worldAxis, float s)
        {
            Ray     ray    = _cam.ScreenPointToRay(Input.mousePosition);
            Vector3 end    = origin + worldAxis * ShaftLength * s;
            float   tol    = HoverAxisTol * s;

            ClosestPointsBetweenLines(
                ray.origin, ray.direction,
                origin, worldAxis,
                out Vector3 onRay, out Vector3 onAxis);

            float t = Vector3.Dot(onAxis - origin, worldAxis);
            if (t < 0f || t > ShaftLength * s) return false;

            return Vector3.Distance(onRay, onAxis) <= tol;
        }

        private bool HoverPlane(Vector3 origin, Vector3 axisA, Vector3 axisB, float s)
        {
            Ray ray = _cam.ScreenPointToRay(Input.mousePosition);
            var plane = new Plane(Vector3.Cross(axisA, axisB).normalized, origin);
            if (!plane.Raycast(ray, out float enter)) return false;

            Vector3 hit   = ray.GetPoint(enter);
            Vector3 local = hit - origin;
            float   projA = Vector3.Dot(local, axisA.normalized);
            float   projB = Vector3.Dot(local, axisB.normalized);
            float   min   = PlaneOffset * s;
            float   max   = (PlaneOffset + PlaneSize) * s;
            return projA >= min && projA <= max && projB >= min && projB <= max;
        }

        private bool HoverRing(Vector3 origin, Vector3 ringAxis, float s)
        {
            Ray ray = _cam.ScreenPointToRay(Input.mousePosition);
            var ringPlane = new Plane(ringAxis, origin);
            if (!ringPlane.Raycast(ray, out float enter)) return false;

            float dist = Vector3.Distance(ray.GetPoint(enter), origin);
            float r    = RotateRadius * s;
            return Mathf.Abs(dist - r) <= HoverRingTol * s;
        }

        // ── Drag ─────────────────────────────────────────────────────────────

        private void StartDrag(float s)
        {
            _active     = _hovered;
            _isDragging = true;

            Transform t      = _target.transform;
            _positionBefore  = t.position;
            _rotationBefore  = t.rotation;
            _scaleBefore     = t.localScale;
            _dragStartPos    = t.position;
            _dragStartScale  = t.localScale;

            switch (_active)
            {
                case Handle.AxisX:   BeginAxisDrag(WorldAxisX); break;
                case Handle.AxisY:   BeginAxisDrag(WorldAxisY); break;
                case Handle.AxisZ:   BeginAxisDrag(WorldAxisZ); break;
                case Handle.PlaneXY:
                    if (_mode == GizmoMode.Scale) BeginScalePlaneDrag(0, 1);
                    else                          BeginPlaneDrag(Vector3.Cross(WorldAxisX, WorldAxisY));
                    break;
                case Handle.PlaneYZ:
                    if (_mode == GizmoMode.Scale) BeginScalePlaneDrag(1, 2);
                    else                          BeginPlaneDrag(Vector3.Cross(WorldAxisY, WorldAxisZ));
                    break;
                case Handle.PlaneXZ:
                    if (_mode == GizmoMode.Scale) BeginScalePlaneDrag(0, 2);
                    else                          BeginPlaneDrag(Vector3.Cross(WorldAxisX, WorldAxisZ));
                    break;
                case Handle.RotateX: BeginRotateDrag(WorldAxisX); break;
                case Handle.RotateY: BeginRotateDrag(WorldAxisY); break;
                case Handle.RotateZ: BeginRotateDrag(WorldAxisZ); break;
            }
        }

        private void BeginAxisDrag(Vector3 worldAxis)
        {
            _dragAxis            = worldAxis;
            Ray ray              = _cam.ScreenPointToRay(Input.mousePosition);
            _dragStartWorldPoint = ClosestPointOnAxisToMouse(ray, _dragStartPos, worldAxis);
        }

        private void BeginPlaneDrag(Vector3 planeNormal)
        {
            _dragPlaneNormal = planeNormal.normalized;
            Ray ray          = _cam.ScreenPointToRay(Input.mousePosition);
            var plane        = new Plane(_dragPlaneNormal, _dragStartPos);
            if (plane.Raycast(ray, out float enter))
                _dragStartWorldPoint = ray.GetPoint(enter);
        }

        private void BeginRotateDrag(Vector3 axis)
        {
            _dragAxis        = axis;
            _dragStartMouseX = Input.mousePosition.x;
            _dragStartRot    = _target.transform.rotation;
        }

        private void BeginScalePlaneDrag(int axisA, int axisB)
        {
            _scaleAxisA      = axisA;
            _scaleAxisB      = axisB;
            _dragStartMousePos = Input.mousePosition;
        }

        private void UpdateDrag()
        {
            switch (_mode)
            {
                case GizmoMode.Translate: UpdateTranslateDrag(); break;
                case GizmoMode.Rotate:    UpdateRotateDrag();    break;
                case GizmoMode.Scale:     UpdateScaleDrag();     break;
            }
        }

        private void UpdateTranslateDrag()
        {
            Ray ray = _cam.ScreenPointToRay(Input.mousePosition);

            if (_active is Handle.PlaneXY or Handle.PlaneYZ or Handle.PlaneXZ)
            {
                var plane = new Plane(_dragPlaneNormal, _dragStartPos);
                if (plane.Raycast(ray, out float enter))
                    _target.transform.position = _dragStartPos + (ray.GetPoint(enter) - _dragStartWorldPoint);
            }
            else
            {
                Vector3 newPoint = ClosestPointOnAxisToMouse(ray, _dragStartPos, _dragAxis);
                _target.transform.position = _dragStartPos + (newPoint - _dragStartWorldPoint);
            }
        }

        private void UpdateRotateDrag()
        {
            float angle = (Input.mousePosition.x - _dragStartMouseX) * RotateSensitivity;
            _target.transform.rotation = Quaternion.AngleAxis(angle, _dragAxis) * _dragStartRot;
        }

        private void UpdateScaleDrag()
        {
            if (_active is Handle.PlaneXY or Handle.PlaneYZ or Handle.PlaneXZ)
            {
                Vector2 delta      = (Vector2)Input.mousePosition - _dragStartMousePos;
                float   sign       = (delta.x + delta.y) >= 0f ? 1f : -1f;
                float   scaleDelta = delta.magnitude * ScaleSensitivity * sign;
                Vector3 scale      = _dragStartScale;
                scale[_scaleAxisA] = Mathf.Max(0.01f, scale[_scaleAxisA] + scaleDelta);
                scale[_scaleAxisB] = Mathf.Max(0.01f, scale[_scaleAxisB] + scaleDelta);
                _target.transform.localScale = scale;
            }
            else
            {
                Ray     ray      = _cam.ScreenPointToRay(Input.mousePosition);
                Vector3 newPoint = ClosestPointOnAxisToMouse(ray, _dragStartPos, _dragAxis);
                float   delta    = Vector3.Dot(newPoint - _dragStartWorldPoint, _dragAxis);
                int     idx      = _active == Handle.AxisX ? 0 : _active == Handle.AxisY ? 1 : 2;
                Vector3 scale    = _dragStartScale;
                scale[idx]       = Mathf.Max(0.01f, scale[idx] + delta);
                _target.transform.localScale = scale;
            }
        }

        private void EndDrag()
        {
            _isDragging = false;
            _active     = Handle.None;

            if (_domainObj == null || _target == null) return;

            Transform t = _target.transform;
            var after   = new TransformModel
            {
                Position = new DomainVec3(t.position.x,   t.position.y,   t.position.z),
                Rotation = new DomainQuat(t.rotation.x,   t.rotation.y,   t.rotation.z, t.rotation.w),
                Scale    = new DomainVec3(t.localScale.x, t.localScale.y, t.localScale.z)
            };

            string label = _mode switch
            {
                GizmoMode.Translate => $"Move {_domainObj.DisplayName}",
                GizmoMode.Rotate    => $"Rotate {_domainObj.DisplayName}",
                GizmoMode.Scale     => $"Scale {_domainObj.DisplayName}",
                _                   => "Transform"
            };

            _vm.TransformObject.Execute(_domainObj, after, label);
        }

        // ── Rendering ─────────────────────────────────────────────────────────

        private void DrawTranslateHandles(Vector3 o, float s)
        {
            DrawAxisArrow(o, WorldAxisX, s, Col(Handle.AxisX,   ColX));
            DrawAxisArrow(o, WorldAxisY, s, Col(Handle.AxisY,   ColY));
            DrawAxisArrow(o, WorldAxisZ, s, Col(Handle.AxisZ,   ColZ));
            DrawPlaneSquare(o, WorldAxisX, WorldAxisY, s, Col(Handle.PlaneXY, ColXY));
            DrawPlaneSquare(o, WorldAxisY, WorldAxisZ, s, Col(Handle.PlaneYZ, ColYZ));
            DrawPlaneSquare(o, WorldAxisX, WorldAxisZ, s, Col(Handle.PlaneXZ, ColXZ));
        }

        private void DrawRotateHandles(Vector3 o, float s)
        {
            GizmoRenderer.DrawWireCircle(o, RotateRadius * s, 64, WorldAxisX, Col(Handle.RotateX, ColX));
            GizmoRenderer.DrawWireCircle(o, RotateRadius * s, 64, WorldAxisY, Col(Handle.RotateY, ColY));
            GizmoRenderer.DrawWireCircle(o, RotateRadius * s, 64, WorldAxisZ, Col(Handle.RotateZ, ColZ));
        }

        private void DrawScaleHandles(Vector3 o, float s)
        {
            DrawAxisCube(o, WorldAxisX, s, Col(Handle.AxisX,   ColX));
            DrawAxisCube(o, WorldAxisY, s, Col(Handle.AxisY,   ColY));
            DrawAxisCube(o, WorldAxisZ, s, Col(Handle.AxisZ,   ColZ));
            DrawPlaneSquare(o, WorldAxisX, WorldAxisY, s, Col(Handle.PlaneXY, ColXY));
            DrawPlaneSquare(o, WorldAxisY, WorldAxisZ, s, Col(Handle.PlaneYZ, ColYZ));
            DrawPlaneSquare(o, WorldAxisX, WorldAxisZ, s, Col(Handle.PlaneXZ, ColXZ));
        }

        private void DrawAxisArrow(Vector3 origin, Vector3 axis, float s, Color col)
        {
            Vector3 tip = origin + axis * ShaftLength * s;
            GizmoRenderer.DrawCylinder(origin, axis, ShaftLength * s, col, ShaftRadius * s);
            GizmoRenderer.DrawCone(tip, axis, col, ArrowLength * s, ArrowRadius * s);
        }

        private void DrawAxisCube(Vector3 origin, Vector3 axis, float s, Color col)
        {
            Vector3 tip = origin + axis * ShaftLength * s;
            GizmoRenderer.DrawCylinder(origin, axis, ShaftLength * s, col, ShaftRadius * s);
            GizmoRenderer.DrawPoint(tip, CubeTipRadius * s, col);
        }

        private void DrawPlaneSquare(Vector3 origin, Vector3 axisA, Vector3 axisB, float s, Color col)
        {
            float min = PlaneOffset * s;
            float max = (PlaneOffset + PlaneSize) * s;

            Vector3 a = origin + axisA * min + axisB * min;
            Vector3 b = origin + axisA * max + axisB * min;
            Vector3 c = origin + axisA * max + axisB * max;
            Vector3 d = origin + axisA * min + axisB * max;

            GizmoRenderer.DrawLine(a, b, col);
            GizmoRenderer.DrawLine(b, c, col);
            GizmoRenderer.DrawLine(c, d, col);
            GizmoRenderer.DrawLine(d, a, col);
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private Color Col(Handle handle, Color defaultCol)
            => (_hovered == handle || _active == handle) ? ColHover : defaultCol;

        private float GizmoScale()
        {
            if (_target == null || _cam == null) return 1f;
            float dist = Vector3.Distance(_cam.transform.position, _target.transform.position);
            return Mathf.Max(0.01f, dist) * GizmoScaleFactor;
        }

        private Vector3 WorldAxisX => _isLocal ? _target.transform.right   : Vector3.right;
        private Vector3 WorldAxisY => _isLocal ? _target.transform.up      : Vector3.up;
        private Vector3 WorldAxisZ => _isLocal ? _target.transform.forward : Vector3.forward;

        // ── Geometry (no external dependency) ────────────────────────────────

        private static Vector3 ClosestPointOnAxisToMouse(Ray mouseRay, Vector3 axisOrigin, Vector3 axisDir)
        {
            Vector3 p1 = mouseRay.origin;
            Vector3 d1 = mouseRay.direction.normalized;
            Vector3 d2 = axisDir.normalized;
            Vector3 r  = axisOrigin - p1;

            Vector3 cross = Vector3.Cross(d1, d2);
            float   denom = cross.sqrMagnitude;
            if (denom < 1e-6f) return axisOrigin;

            float t = Vector3.Dot(Vector3.Cross(r, d1), cross) / denom;
            return axisOrigin + d2 * t;
        }

        private static void ClosestPointsBetweenLines(
            Vector3 p1, Vector3 d1, Vector3 p2, Vector3 d2,
            out Vector3 point1, out Vector3 point2)
        {
            float a = Vector3.Dot(d1, d1);
            float b = Vector3.Dot(d1, d2);
            float e = Vector3.Dot(d2, d2);
            float d = a * e - b * b;

            Vector3 r = p1 - p2;
            float   c = Vector3.Dot(d1, r);
            float   f = Vector3.Dot(d2, r);

            float s = (b * f - c * e) / d;
            float t = (a * f - c * b) / d;

            point1 = p1 + d1 * s;
            point2 = p2 + d2 * t;
        }
    }
}
