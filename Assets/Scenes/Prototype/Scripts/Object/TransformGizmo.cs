using System.Collections.Generic;
using UnityEngine;
using VTT.UI;

namespace VTT
{
    /// <summary>
    /// Full 3D transform gizmo for DecorObjects.
    ///
    /// BUGS FIXED vs previous version:
    ///   1. Pink materials  → shader detection now tries URP/Unlit before Standard
    ///   2. Inverted motion → GetAxisOffset sign was negated, now correct
    ///   3. Z arrow         → Euler(-90,0,0) confirmed correct for +Z
    ///
    /// NEW:
    ///   • Hover outline  (scaled-up bright mesh child per handle)
    ///   • Center sphere  (XYZ translate on view-plane | uniform scale)
    ///   • Dual-axis planes  XY / XZ / YZ (small quads, blend two axes)
    ///
    /// Keyboard: W = Translate | E = Rotate | R = Scale | Q = Local/World | Del = Delete
    /// </summary>
    [AddComponentMenu("VTT/Transform Gizmo")]
    public class TransformGizmo : MonoBehaviour
    {
        // ── Inspector ─────────────────────────────────────────────────────────
        [Header("Layers")]
        [SerializeField] private int       gizmoLayer   = 30;
        [SerializeField] private LayerMask selectLayer  = ~0;

        [Header("Appearance")]
        [SerializeField] private float screenSizeScale = 0.13f;
        [SerializeField] private float shaftRadius     = 0.035f;
        [SerializeField] private float arrowTipH       = 0.26f;
        [SerializeField] private float arrowTipR       = 0.09f;
        [SerializeField] private float ringMajorR      = 1.00f;
        [SerializeField] private float ringMinorR      = 0.038f;
        [SerializeField] private float scaleCubeSize   = 0.16f;
        [SerializeField] private float centerSphereR   = 0.12f;
        [SerializeField] private float dualQuadSize    = 0.22f;
        [SerializeField] private float dualQuadOffset  = 0.38f;
        [SerializeField] private float outlineScale    = 1.18f;

        // ── Singleton ─────────────────────────────────────────────────────────
        public static TransformGizmo Instance { get; private set; }

        // ── Mode ──────────────────────────────────────────────────────────────
        public enum GizmoMode { Translate, Rotate, Scale }
        public GizmoMode Mode    { get; private set; } = GizmoMode.Translate;
        public bool      IsLocal { get; private set; } = false;

        // ── Selection ─────────────────────────────────────────────────────────
        public static DecorObject Selected { get; private set; }
        public static void Select(DecorObject d) { Selected = d; }
        public static void Deselect()            { Selected = null; }

        // ── Axis enum (extended) ──────────────────────────────────────────────
        private enum Axis { X, Y, Z, XY, XZ, YZ, XYZ, None }
        private Axis _hovered  = Axis.None;
        private Axis _dragging = Axis.None;

        // ── Colors ────────────────────────────────────────────────────────────
        private static readonly Color CX    = new(0.90f, 0.16f, 0.16f, 1f);
        private static readonly Color CY    = new(0.16f, 0.80f, 0.16f, 1f);
        private static readonly Color CZ    = new(0.16f, 0.38f, 0.90f, 1f);
        private static readonly Color CXY   = new(0.90f, 0.90f, 0.10f, 0.55f);  // yellow
        private static readonly Color CXZ   = new(0.80f, 0.10f, 0.80f, 0.55f);  // magenta
        private static readonly Color CYZ   = new(0.10f, 0.85f, 0.85f, 0.55f);  // cyan
        private static readonly Color CXYZ  = new(0.95f, 0.95f, 0.95f, 1f);     // white center
        private static readonly Color COUTL = new(0.98f, 0.92f, 0.18f, 1f);     // hover outline

        // ── Per-handle bookkeeping ─────────────────────────────────────────────
        private class Handle
        {
            public GameObject go;
            public GameObject outline;    // slightly larger child for hover ring
            public MeshRenderer mr;
            public MeshRenderer outlineMr;
            public Collider     col;
            public Material     normalMat;
        }

        // indexed by (int)Axis  0–6
        private Handle[] _transHandles;  // translate: X Y Z XY XZ YZ XYZ
        private Handle[] _rotHandles;    // rotate:    X Y Z
        private Handle[] _scaleHandles;  // scale:     X Y Z XYZ

        private GameObject _gizmoRoot;

        // ── Materials ─────────────────────────────────────────────────────────
        private Material _outlineMat;   // shared bright yellow unlit
        private Material[] _dualMats;   // semi-transparent dual-axis

        // ── Drag state ────────────────────────────────────────────────────────
        private Vector3    _dragPosStart;
        private Quaternion _dragRotStart;
        private Vector3    _dragScaleStart;
        private float      _dragOffset;       // single-axis translate/scale
        private float      _dragAngle;        // rotate
        private Vector3    _dragPlaneHitStart;// dual-axis / XYZ

        // ── Lifecycle ─────────────────────────────────────────────────────────
        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            _outlineMat = MakeMat(COUTL, false);
            BuildAllHandles();
            ShowMode();
        }

        private void Update()
        {
            if (!VTTPanelUI.IsMouseOverUI)
            {
                if (Input.GetKeyDown(KeyCode.W)) SetMode(GizmoMode.Translate);
                if (Input.GetKeyDown(KeyCode.E)) SetMode(GizmoMode.Rotate);
                if (Input.GetKeyDown(KeyCode.R)) SetMode(GizmoMode.Scale);
                if (Input.GetKeyDown(KeyCode.Q)) { IsLocal = !IsLocal; }
                if (Input.GetKeyDown(KeyCode.Delete) && Selected != null) DeleteSelected();
            }

            if (Selected == null || !Selected.gameObject.activeInHierarchy)
            {
                HideAll();
                if (_dragging != Axis.None) _dragging = Axis.None;
                return;
            }

            SyncGizmoTransform();
            ShowMode();

            if (_dragging != Axis.None)
            {
                if (Input.GetMouseButtonUp(0))
            {
                // Record undo command — only if transform actually changed
                if (Selected != null)
                {
                    var t = Selected.transform;
                    bool changed = t.position    != _dragPosStart ||
                                   t.rotation    != _dragRotStart ||
                                   t.localScale  != _dragScaleStart;
                    if (changed)
                    {
                        string label = Mode switch
                        {
                            GizmoMode.Rotate => $"Rotate {Selected.displayName}",
                            GizmoMode.Scale  => $"Scale {Selected.displayName}",
                            _                => $"Move {Selected.displayName}"
                        };
                        CommandHistory.Instance?.Record(new TransformCommand(
                            t,
                            _dragPosStart,   _dragRotStart,   _dragScaleStart,
                            t.position,      t.rotation,      t.localScale,
                            label));
                    }
                }
                _dragging = Axis.None; UpdateHover();
            }
                else                            ContinueDrag();
            }
            else
            {
                UpdateHover();
                if (Input.GetMouseButtonDown(0) && !VTTPanelUI.IsMouseOverUI)
                    TryStartDrag();
            }
        }

        // ── Transform sync ────────────────────────────────────────────────────
        private void SyncGizmoTransform()
        {
            float dist = Vector3.Distance(Camera.main.transform.position,
                                          Selected.transform.position);
            float size = dist * screenSizeScale;
            _gizmoRoot.transform.position   = Selected.transform.position;
            _gizmoRoot.transform.rotation   = IsLocal
                ? Selected.transform.rotation : Quaternion.identity;
            _gizmoRoot.transform.localScale = Vector3.one * size;
        }

        // ── Mode ──────────────────────────────────────────────────────────────
        private void SetMode(GizmoMode m) { Mode = m; ShowMode(); }

        private void ShowMode()
        {
            bool show = Selected != null;
            SetHandlesActive(_transHandles, show && Mode == GizmoMode.Translate);
            SetHandlesActive(_rotHandles,   show && Mode == GizmoMode.Rotate);
            SetHandlesActive(_scaleHandles, show && Mode == GizmoMode.Scale);
        }

        private void HideAll()
        {
            SetHandlesActive(_transHandles, false);
            SetHandlesActive(_rotHandles,   false);
            SetHandlesActive(_scaleHandles, false);
        }

        private void SetHandlesActive(Handle[] handles, bool active)
        {
            if (handles == null) return;
            foreach (var h in handles) if (h?.go != null) h.go.SetActive(active);
        }

        // ── Hover ─────────────────────────────────────────────────────────────
        private void UpdateHover()
        {
            Axis prev    = _hovered;
            _hovered     = Axis.None;
            var activeH  = ActiveHandles();
            if (activeH == null) return;

            var ray  = Camera.main.ScreenPointToRay(Input.mousePosition);
            float best = Mathf.Infinity;

            foreach (var h in activeH)
            {
                if (h?.col == null || !h.go.activeInHierarchy) continue;
                if (h.col.Raycast(ray, out RaycastHit hit, Mathf.Infinity)
                    && hit.distance < best)
                {
                    best     = hit.distance;
                    _hovered = HandleToAxis(h, activeH);
                }
            }

            if (_hovered != prev) ApplyHoverVisuals(activeH);
        }

        private void ApplyHoverVisuals(Handle[] handles)
        {
            foreach (var h in handles)
            {
                if (h == null) continue;
                bool hot = HandleToAxis(h, handles) == _hovered && _hovered != Axis.None;

                // Outline child: visible only when hovered
                if (h.outline != null) h.outline.SetActive(hot);

                // Scale the handle slightly on hover for extra clarity
                if (h.go != null)
                    h.go.transform.localScale = hot ? Vector3.one * 1.08f : Vector3.one;
            }
        }

        // ── Drag start ────────────────────────────────────────────────────────
        private void TryStartDrag()
        {
            if (_hovered == Axis.None) { TrySelect(); return; }

            _dragging       = _hovered;
            _dragPosStart   = Selected.transform.position;
            _dragRotStart   = Selected.transform.rotation;
            _dragScaleStart = Selected.transform.localScale;

            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            switch (_hovered)
            {
                case Axis.X:
                case Axis.Y:
                case Axis.Z:
                    if (Mode == GizmoMode.Rotate)
                    {
                        Vector3 planeNorm = GetSingleAxis(_hovered);
                        Vector3 hit       = RayPlane(ray, _dragPosStart, planeNorm);
                        Vector3 ref1      = Mathf.Abs(Vector3.Dot(planeNorm, Vector3.up)) > 0.99f
                                            ? Vector3.forward : Vector3.up;
                        Vector3 fwd       = Vector3.Cross(planeNorm, ref1).normalized;
                        Vector3 up2       = Vector3.Cross(fwd, planeNorm).normalized;
                        Vector3 delta     = hit - _dragPosStart;
                        _dragAngle = Mathf.Atan2(Vector3.Dot(delta, fwd),
                                                  Vector3.Dot(delta, up2)) * Mathf.Rad2Deg;
                    }
                    else
                    {
                        _dragOffset = AxisOffset(GetSingleAxis(_hovered), _dragPosStart);
                    }
                    break;

                case Axis.XY:
                case Axis.XZ:
                case Axis.YZ:
                    _dragPlaneHitStart = RayPlane(ray, _dragPosStart, DualPlaneNormal(_hovered));
                    break;

                case Axis.XYZ:
                    // View-plane for translate, any plane for scale
                    _dragPlaneHitStart = RayPlane(ray, _dragPosStart,
                        Camera.main.transform.forward);
                    if (Mode == GizmoMode.Scale)
                        _dragOffset = 0f;
                    break;
            }
        }

        // ── Drag continue ─────────────────────────────────────────────────────
        private void ContinueDrag()
        {
            if (Selected == null) return;
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            switch (_dragging)
            {
                // ── Single axis ───────────────────────────────────────────────
                case Axis.X:
                case Axis.Y:
                case Axis.Z:
                    Vector3 singleAxis = GetSingleAxis(_dragging);
                    if (Mode == GizmoMode.Translate)
                    {
                        float t     = AxisOffset(singleAxis, _dragPosStart);
                        float delta = t - _dragOffset;
                        Selected.transform.position = _dragPosStart + singleAxis * delta;
                    }
                    else if (Mode == GizmoMode.Scale)
                    {
                        float t     = AxisOffset(singleAxis, _dragPosStart);
                        float delta = (t - _dragOffset) * 1.5f;
                        var   s     = _dragScaleStart;
                        Selected.transform.localScale = _dragging switch
                        {
                            Axis.X => new Vector3(Mathf.Max(0.01f, s.x + delta), s.y, s.z),
                            Axis.Y => new Vector3(s.x, Mathf.Max(0.01f, s.y + delta), s.z),
                            _      => new Vector3(s.x, s.y, Mathf.Max(0.01f, s.z + delta))
                        };
                    }
                    else // Rotate
                    {
                        Vector3 planeNorm = singleAxis;
                        Vector3 hit       = RayPlane(ray, _dragPosStart, planeNorm);
                        Vector3 ref1      = Mathf.Abs(Vector3.Dot(planeNorm, Vector3.up)) > 0.99f
                                            ? Vector3.forward : Vector3.up;
                        Vector3 fwd       = Vector3.Cross(planeNorm, ref1).normalized;
                        Vector3 up2       = Vector3.Cross(fwd, planeNorm).normalized;
                        Vector3 d2        = hit - _dragPosStart;
                        float   angle     = Mathf.Atan2(Vector3.Dot(d2, fwd),
                                                         Vector3.Dot(d2, up2)) * Mathf.Rad2Deg;
                        float   dAngle    = angle - _dragAngle;
                        Selected.transform.rotation =
                            Quaternion.AngleAxis(dAngle, singleAxis) * _dragRotStart;
                    }
                    break;

                // ── Dual axis ─────────────────────────────────────────────────
                case Axis.XY:
                case Axis.XZ:
                case Axis.YZ:
                    if (Mode == GizmoMode.Translate)
                    {
                        Vector3 hitNow = RayPlane(ray, _dragPosStart, DualPlaneNormal(_dragging));
                        Vector3 move   = hitNow - _dragPlaneHitStart;
                        // Project onto the two allowed axes only
                        Vector3 a1 = DualAxis1(_dragging), a2 = DualAxis2(_dragging);
                        move = a1 * Vector3.Dot(move, a1) + a2 * Vector3.Dot(move, a2);
                        Selected.transform.position = _dragPosStart + move;
                    }
                    break;

                // ── All axes ──────────────────────────────────────────────────
                case Axis.XYZ:
                    if (Mode == GizmoMode.Translate)
                    {
                        Vector3 hitNow = RayPlane(ray, _dragPosStart,
                            Camera.main.transform.forward);
                        Selected.transform.position = _dragPosStart + (hitNow - _dragPlaneHitStart);
                    }
                    else if (Mode == GizmoMode.Scale)
                    {
                        // Drag right = bigger, left = smaller
                        Vector3 hitNow = RayPlane(ray, _dragPosStart,
                            Camera.main.transform.forward);
                        float screenDelta = Vector3.Dot(hitNow - _dragPlaneHitStart,
                            Camera.main.transform.right);
                        float factor = 1f + screenDelta;
                        factor = Mathf.Max(0.01f, factor);
                        Selected.transform.localScale = _dragScaleStart * factor;
                    }
                    break;
            }
        }

        // ── Selection ─────────────────────────────────────────────────────────
        private void TrySelect()
        {
            var ray  = Camera.main.ScreenPointToRay(Input.mousePosition);
            int mask = ~(1 << gizmoLayer);
            if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, mask))
            { Deselect(); return; }
            Select(hit.collider.GetComponentInParent<DecorObject>());
        }

        public void DeleteSelected()
        {
            if (Selected == null) return;
            var d = Selected;
            // Record BEFORE deleting so the command holds a live reference
            CommandHistory.Instance?.Record(new DeleteCommand(d));
            Deselect();
            // Park the GO inactive instead of destroying — DeleteCommand needs it for Undo
            var ps = Grid.PlacementSystem.Instance;
            var po = d.GetComponent<Grid.PlaceableObject>();
            if (ps != null && po != null) ps.Remove(po);
            d.gameObject.SetActive(false);
        }

        // ── Axis math helpers ─────────────────────────────────────────────────

        /// <summary>Correct closest-point-on-axisRay formula (fixed sign vs old version).</summary>
        private float AxisOffset(Vector3 axisDir, Vector3 axisOrigin)
        {
            Ray   mRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            Vector3 w  = mRay.origin - axisOrigin;
            float   b  = Vector3.Dot(mRay.direction, axisDir);
            float   d  = Vector3.Dot(mRay.direction, w);
            float   e  = Vector3.Dot(axisDir,        w);
            float   D  = 1f - b * b;
            // FIX: was (b*d - e)/D which is negated. Correct is (e - b*d)/D.
            return Mathf.Abs(D) < 1e-5f ? e : (e - b * d) / D;
        }

        private static Vector3 RayPlane(Ray ray, Vector3 pt, Vector3 normal)
        {
            float denom = Vector3.Dot(normal, ray.direction);
            if (Mathf.Abs(denom) < 1e-5f) return ray.GetPoint(10f);
            float t = Vector3.Dot(normal, pt - ray.origin) / denom;
            return ray.GetPoint(Mathf.Max(0f, t));
        }

        private Vector3 GetSingleAxis(Axis a)
        {
            if (IsLocal && Selected != null)
                return a switch
                {
                    Axis.X => Selected.transform.right,
                    Axis.Y => Selected.transform.up,
                    _      => Selected.transform.forward
                };
            return a switch { Axis.X => Vector3.right, Axis.Y => Vector3.up, _ => Vector3.forward };
        }

        // Plane normal for dual-axis handle = the axis NOT in the pair
        private Vector3 DualPlaneNormal(Axis a) => a switch
        {
            Axis.XY => IsLocal && Selected != null ? Selected.transform.forward : Vector3.forward,
            Axis.XZ => IsLocal && Selected != null ? Selected.transform.up      : Vector3.up,
            _       => IsLocal && Selected != null ? Selected.transform.right   : Vector3.right
        };

        private Vector3 DualAxis1(Axis a) => a switch
        {
            Axis.XY => GetSingleAxis(Axis.X), Axis.XZ => GetSingleAxis(Axis.X),
            _       => GetSingleAxis(Axis.Y)
        };
        private Vector3 DualAxis2(Axis a) => a switch
        {
            Axis.XY => GetSingleAxis(Axis.Y), Axis.XZ => GetSingleAxis(Axis.Z),
            _       => GetSingleAxis(Axis.Z)
        };

        // ── Handle ↔ Axis mapping ──────────────────────────────────────────────
        private Axis HandleToAxis(Handle h, Handle[] arr)
        {
            for (int i = 0; i < arr.Length; i++)
                if (arr[i] == h) return (Axis)i;
            return Axis.None;
        }

        private Handle[] ActiveHandles() => Mode switch
        {
            GizmoMode.Rotate => _rotHandles,
            GizmoMode.Scale  => _scaleHandles,
            _                => _transHandles
        };

        // ── Build all handles ─────────────────────────────────────────────────
        private void BuildAllHandles()
        {
            _gizmoRoot = new GameObject("_GizmoRoot") { hideFlags = HideFlags.HideInHierarchy };

            // Translate: X Y Z XY XZ YZ XYZ  (indices 0-6)
            _transHandles = new Handle[7];
            for (int i = 0; i < 3; i++) _transHandles[i]     = BuildArrow(i);
            _transHandles[3] = BuildDualQuad(Axis.XY);
            _transHandles[4] = BuildDualQuad(Axis.XZ);
            _transHandles[5] = BuildDualQuad(Axis.YZ);
            _transHandles[6] = BuildCenterSphere(CXYZ);

            // Rotate: X Y Z  (indices 0-2, padded to 7)
            _rotHandles = new Handle[7];
            for (int i = 0; i < 3; i++) _rotHandles[i] = BuildRing(i);

            // Scale: X Y Z XYZ  (indices 0, 1, 2, 6)
            _scaleHandles = new Handle[7];
            for (int i = 0; i < 3; i++) _scaleHandles[i] = BuildScaleHandle(i);
            _scaleHandles[6] = BuildCenterSphere(CXYZ);
        }

        // ── Arrow ─────────────────────────────────────────────────────────────
        private Handle BuildArrow(int axis)
        {
            Color col = AxisColor(axis);
            var root  = MkGO($"Arrow_{AxisName(axis)}", _gizmoRoot.transform, axis);
            root.transform.localRotation = AxisRotation(axis);

            // Shaft (cylinder 0..1 along local Y)
            var shaft = MkGO("Shaft", root.transform, axis);
            shaft.transform.localPosition = new Vector3(0, 0.5f, 0);
            shaft.AddComponent<MeshFilter>().mesh = MakeCylinder(shaftRadius, 1f, 10);
            shaft.AddComponent<MeshRenderer>().sharedMaterial = MakeMat(col);

            // Cone tip
            var tip = MkGO("Tip", root.transform, axis);
            tip.transform.localPosition = new Vector3(0, 1f, 0);
            tip.AddComponent<MeshFilter>().mesh = MakeCone(arrowTipR, arrowTipH, 10);
            tip.AddComponent<MeshRenderer>().sharedMaterial = MakeMat(col);

            // Collider covers shaft + tip
            var col2 = root.AddComponent<BoxCollider>();
            col2.center = new Vector3(0, 0.6f, 0);
            col2.size   = new Vector3(arrowTipR * 2.4f, 1.3f, arrowTipR * 2.4f);

            // Outline child (slightly bigger shaft, hidden by default)
            var outlineGO = BuildOutlineChild(root.transform, axis,
                new Vector3(0, 0.55f, 0), MakeCylinder(shaftRadius * outlineScale, 1.1f, 10));

            var h          = new Handle();
            h.go           = root;
            h.outline      = outlineGO;
            h.mr           = shaft.GetComponent<MeshRenderer>();
            h.outlineMr    = outlineGO?.GetComponent<MeshRenderer>();
            h.col          = col2;
            h.normalMat    = h.mr.sharedMaterial;
            outlineGO?.SetActive(false);
            return h;
        }

        // ── Ring ──────────────────────────────────────────────────────────────
        private Handle BuildRing(int axis)
        {
            Color col = AxisColor(axis);
            var go    = MkGO($"Ring_{AxisName(axis)}", _gizmoRoot.transform, axis);
            go.transform.localRotation = AxisRotation(axis);

            var mf = go.AddComponent<MeshFilter>();
            var mr = go.AddComponent<MeshRenderer>();
            mf.mesh = MakeTorus(ringMajorR, ringMinorR, 56, 10);
            mr.sharedMaterial = MakeMat(col);

            var mc = go.AddComponent<MeshCollider>();
            mc.sharedMesh = mf.mesh;

            // Outline: bigger torus, hidden by default
            var outGO = MkGO("Outline", go.transform, axis);
            outGO.AddComponent<MeshFilter>().mesh =
                MakeTorus(ringMajorR * outlineScale, ringMinorR * outlineScale, 56, 10);
            var outMR = outGO.AddComponent<MeshRenderer>();
            outMR.sharedMaterial = _outlineMat;
            outGO.SetActive(false);

            var h = new Handle { go = go, outline = outGO, mr = mr,
                                  outlineMr = outMR, col = mc, normalMat = mr.sharedMaterial };
            return h;
        }

        // ── Scale handle ──────────────────────────────────────────────────────
        private Handle BuildScaleHandle(int axis)
        {
            Color col = AxisColor(axis);
            var root  = MkGO($"Scale_{AxisName(axis)}", _gizmoRoot.transform, axis);
            root.transform.localRotation = AxisRotation(axis);

            var shaft = MkGO("Shaft", root.transform, axis);
            shaft.transform.localPosition = new Vector3(0, 0.5f, 0);
            shaft.AddComponent<MeshFilter>().mesh = MakeCylinder(shaftRadius, 1f, 10);
            shaft.AddComponent<MeshRenderer>().sharedMaterial = MakeMat(col);

            var cube = MkGO("Cube", root.transform, axis);
            cube.transform.localPosition = new Vector3(0, 1f + scaleCubeSize * 0.5f, 0);
            cube.AddComponent<MeshFilter>().mesh = MakeCube(scaleCubeSize);
            cube.AddComponent<MeshRenderer>().sharedMaterial = MakeMat(col);

            var col2 = root.AddComponent<BoxCollider>();
            col2.center = new Vector3(0, 0.6f, 0);
            col2.size   = new Vector3(scaleCubeSize * 1.6f, 1.25f, scaleCubeSize * 1.6f);

            var outlineGO = BuildOutlineChild(root.transform, axis,
                new Vector3(0, 0.6f, 0), MakeCylinder(shaftRadius * outlineScale, 1f, 10));

            var h = new Handle { go = root, outline = outlineGO,
                mr = shaft.GetComponent<MeshRenderer>(),
                outlineMr = outlineGO?.GetComponent<MeshRenderer>(),
                col = col2, normalMat = shaft.GetComponent<MeshRenderer>().sharedMaterial };
            outlineGO?.SetActive(false);
            return h;
        }

        // ── Dual-axis quad ────────────────────────────────────────────────────
        private Handle BuildDualQuad(Axis dual)
        {
            Color col = dual switch { Axis.XY => CXY, Axis.XZ => CXZ, _ => CYZ };
            var go    = MkGO($"Plane_{dual}", _gizmoRoot.transform, -1);

            // Position in the corner of the two axes
            go.transform.localPosition = DualQuadLocalPos(dual);
            go.transform.localRotation = DualQuadLocalRot(dual);

            var mat = MakeMat(col, true);
            go.AddComponent<MeshFilter>().mesh = MakeQuad(dualQuadSize);
            var mr = go.AddComponent<MeshRenderer>();
            mr.sharedMaterial = mat;

            var bc = go.AddComponent<BoxCollider>();
            bc.size = new Vector3(dualQuadSize, dualQuadSize, 0.02f);

            // Outline
            var outGO = MkGO("Outline", go.transform, -1);
            outGO.AddComponent<MeshFilter>().mesh = MakeQuad(dualQuadSize * outlineScale);
            var outMR = outGO.AddComponent<MeshRenderer>();
            outMR.sharedMaterial = _outlineMat;
            outGO.SetActive(false);

            return new Handle { go = go, outline = outGO, mr = mr,
                                 outlineMr = outMR, col = bc, normalMat = mat };
        }

        private Vector3 DualQuadLocalPos(Axis dual) => dual switch
        {
            Axis.XY => new Vector3( dualQuadOffset,  dualQuadOffset, 0),
            Axis.XZ => new Vector3( dualQuadOffset, 0,  dualQuadOffset),
            _       => new Vector3(0,  dualQuadOffset,  dualQuadOffset)
        };
        private Quaternion DualQuadLocalRot(Axis dual) => dual switch
        {
            Axis.XY => Quaternion.identity,
            Axis.XZ => Quaternion.Euler(90, 0, 0),
            _       => Quaternion.Euler(0, 90, 0)
        };

        // ── Center sphere ─────────────────────────────────────────────────────
        private Handle BuildCenterSphere(Color col)
        {
            var go  = MkGO("Center", _gizmoRoot.transform, -1);
            var mat = MakeMat(col);
            go.AddComponent<MeshFilter>().mesh = MakeSphere(centerSphereR, 16, 12);
            var mr = go.AddComponent<MeshRenderer>();
            mr.sharedMaterial = mat;
            var sc = go.AddComponent<SphereCollider>();
            sc.radius = centerSphereR;

            // Outline sphere
            var outGO = MkGO("Outline", go.transform, -1);
            outGO.AddComponent<MeshFilter>().mesh =
                MakeSphere(centerSphereR * outlineScale, 16, 12);
            var outMR = outGO.AddComponent<MeshRenderer>();
            outMR.sharedMaterial = _outlineMat;
            outGO.SetActive(false);

            return new Handle { go = go, outline = outGO, mr = mr,
                                 outlineMr = outMR, col = sc, normalMat = mat };
        }

        // ── Outline child helper ──────────────────────────────────────────────
        private GameObject BuildOutlineChild(Transform parent, int axis,
                                              Vector3 localPos, Mesh mesh)
        {
            var go = MkGO("Outline", parent, axis);
            go.transform.localPosition = localPos;
            go.AddComponent<MeshFilter>().mesh = mesh;
            go.AddComponent<MeshRenderer>().sharedMaterial = _outlineMat;
            return go;
        }

        // ── Materials ─────────────────────────────────────────────────────────
        /// <summary>
        /// Creates a material that:
        ///  - Works in Built-in RP, URP, and HDRP
        ///  - Renders ON TOP of every mesh (ZTest Always, renderQueue 5000)
        ///  - Does NOT write to depth so handles don't occlude each other
        /// </summary>
        private static Material MakeMat(Color col, bool transparent = false)
        {
            Shader sh = Shader.Find("Universal Render Pipeline/Unlit")
                     ?? Shader.Find("Unlit/Color")
                     ?? Shader.Find("Standard")
                     ?? Shader.Find("Diffuse");

            var m = new Material(sh);

            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", col);
            if (m.HasProperty("_Color"))     m.SetColor("_Color",     col);

            // renderQueue 5000 = overlay tier, draws after all opaque + transparent geometry
            m.renderQueue = 5000;
            // ZTest Always: gizmo is visible even when behind meshes
            m.SetInt("_ZTest",  (int)UnityEngine.Rendering.CompareFunction.Always);
            // Never write depth so handles don't clip each other
            m.SetInt("_ZWrite", 0);

            if (transparent || col.a < 1f)
            {
                if (m.HasProperty("_Surface"))   m.SetFloat("_Surface",   1);
                if (m.HasProperty("_Mode"))      m.SetFloat("_Mode",      3);
                if (m.HasProperty("_AlphaClip")) m.SetFloat("_AlphaClip", 0);
                m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                m.EnableKeyword("_ALPHABLEND_ON");
            }
            else
            {
                if (m.HasProperty("_Surface")) m.SetFloat("_Surface", 0);
                if (m.HasProperty("_Mode"))    m.SetFloat("_Mode",    0);
                m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
            }

            return m;
        }

        // ── Procedural meshes ─────────────────────────────────────────────────
        private static Mesh MakeCylinder(float r, float h, int segs)
        {
            var v = new List<Vector3>(); var t = new List<int>();
            float half = h * 0.5f;
            for (int i = 0; i < segs; i++)
            {
                float a0 = i / (float)segs * Mathf.PI * 2f;
                float a1 = (i+1) / (float)segs * Mathf.PI * 2f;
                float x0 = Mathf.Cos(a0)*r, z0 = Mathf.Sin(a0)*r;
                float x1 = Mathf.Cos(a1)*r, z1 = Mathf.Sin(a1)*r;
                int b = v.Count;
                v.Add(new Vector3(x0,-half,z0)); v.Add(new Vector3(x1,-half,z1));
                v.Add(new Vector3(x1, half,z1)); v.Add(new Vector3(x0, half,z0));
                t.AddRange(new[]{b,b+2,b+1, b,b+3,b+2});
            }
            var m = new Mesh{name="Cyl"};
            m.SetVertices(v); m.SetTriangles(t,0);
            m.RecalculateNormals(); m.RecalculateBounds(); return m;
        }

        private static Mesh MakeCone(float r, float h, int segs)
        {
            var v = new List<Vector3>(); var t = new List<int>();
            var apex = new Vector3(0, h, 0);
            for (int i = 0; i < segs; i++)
            {
                float a0 = i/(float)segs*Mathf.PI*2f, a1=(i+1)/(float)segs*Mathf.PI*2f;
                var v0 = new Vector3(Mathf.Cos(a0)*r,0,Mathf.Sin(a0)*r);
                var v1 = new Vector3(Mathf.Cos(a1)*r,0,Mathf.Sin(a1)*r);
                int b = v.Count;
                v.Add(v0); v.Add(v1); v.Add(apex);
                t.AddRange(new[]{b,b+1,b+2});
                int c = v.Count;
                v.Add(v0); v.Add(Vector3.zero); v.Add(v1);
                t.AddRange(new[]{c,c+1,c+2});
            }
            var m = new Mesh{name="Cone"};
            m.SetVertices(v); m.SetTriangles(t,0);
            m.RecalculateNormals(); m.RecalculateBounds(); return m;
        }

        private static Mesh MakeCube(float s)
        {
            float h = s*0.5f;
            var m = new Mesh{name="Cube",
                vertices = new[]{
                    new Vector3(-h,-h,-h),new Vector3(h,-h,-h),new Vector3(h,h,-h),new Vector3(-h,h,-h),
                    new Vector3(-h,-h, h),new Vector3(h,-h, h),new Vector3(h,h, h),new Vector3(-h,h, h)},
                triangles = new[]{0,2,1,0,3,2, 4,5,6,4,6,7, 0,1,5,0,5,4,
                                   2,3,7,2,7,6, 0,4,7,0,7,3, 1,2,6,1,6,5}};
            m.RecalculateNormals(); m.RecalculateBounds(); return m;
        }

        private static Mesh MakeTorus(float majR, float minR, int majSegs, int minSegs)
        {
            var v = new List<Vector3>(); var t = new List<int>();
            for (int ma = 0; ma <= majSegs; ma++)
            {
                float maj = ma/(float)majSegs*Mathf.PI*2f;
                var   ctr = new Vector3(Mathf.Cos(maj)*majR, 0, Mathf.Sin(maj)*majR);
                var   out2 = new Vector3(Mathf.Cos(maj), 0, Mathf.Sin(maj));
                for (int mi = 0; mi <= minSegs; mi++)
                {
                    float min2 = mi/(float)minSegs*Mathf.PI*2f;
                    v.Add(ctr + out2*(Mathf.Cos(min2)*minR) + Vector3.up*(Mathf.Sin(min2)*minR));
                }
            }
            int mn1 = minSegs+1;
            for (int ma = 0; ma < majSegs; ma++)
                for (int mi = 0; mi < minSegs; mi++)
                {
                    int a = ma*mn1+mi, b = a+mn1;
                    t.AddRange(new[]{a,b,a+1, a+1,b,b+1});
                }
            var m = new Mesh{name="Torus"};
            m.SetVertices(v); m.SetTriangles(t,0);
            m.RecalculateNormals(); m.RecalculateBounds(); return m;
        }

        private static Mesh MakeQuad(float size)
        {
            float h = size * 0.5f;
            var m = new Mesh { name = "Quad",
                vertices  = new[]{ new Vector3(-h,-h,0), new Vector3(h,-h,0),
                                    new Vector3(h, h,0), new Vector3(-h, h,0) },
                triangles = new[]{ 0,2,1, 0,3,2, 0,1,2, 0,2,3 },   // double-sided
                uv        = new[]{ Vector2.zero, Vector2.right, Vector2.one, Vector2.up }
            };
            m.RecalculateNormals(); m.RecalculateBounds(); return m;
        }

        private static Mesh MakeSphere(float r, int latSegs, int lonSegs)
        {
            var v = new List<Vector3>(); var t = new List<int>();
            for (int lat = 0; lat <= latSegs; lat++)
            {
                float theta  = lat / (float)latSegs * Mathf.PI;
                float sinT   = Mathf.Sin(theta), cosT = Mathf.Cos(theta);
                for (int lon = 0; lon <= lonSegs; lon++)
                {
                    float phi = lon / (float)lonSegs * Mathf.PI * 2f;
                    v.Add(new Vector3(sinT * Mathf.Cos(phi), cosT, sinT * Mathf.Sin(phi)) * r);
                }
            }
            int ln1 = lonSegs + 1;
            for (int lat = 0; lat < latSegs; lat++)
                for (int lon = 0; lon < lonSegs; lon++)
                {
                    int a = lat*ln1+lon, b = a+ln1;
                    t.AddRange(new[]{a,b,a+1, a+1,b,b+1});
                }
            var m = new Mesh{name="Sphere"};
            m.SetVertices(v); m.SetTriangles(t,0);
            m.RecalculateNormals(); m.RecalculateBounds(); return m;
        }

        // ── Utility ───────────────────────────────────────────────────────────
        private Color AxisColor(int i) => i switch { 0=>CX, 1=>CY, _=>CZ };
        private static string     AxisName(int i)     => i==0?"X":i==1?"Y":"Z";

        /// <summary>
        /// FIX: Z axis now uses Euler(-90,0,0) — previously +90 made it point -Z.
        /// Euler(0,0,-90) → local Y points to +X  ✓
        /// Euler(0,0,0)   → local Y points to +Y  ✓
        /// Euler(-90,0,0) → local Y points to +Z  ✓ (was +90 → -Z before)
        /// </summary>
        private static Quaternion AxisRotation(int i) => i switch
        {
            0 => Quaternion.Euler(0,  0, -90),
            1 => Quaternion.identity,
            _ => Quaternion.Euler(-90, 0,   0)   // FIX: was +90
        };

        private GameObject MkGO(string name, Transform parent, int layer)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            if (layer >= 0) go.layer = gizmoLayer;
            return go;
        }

        // ── HUD ───────────────────────────────────────────────────────────────
        private GUIStyle _hud;
        private void OnGUI()
        {
            if (_hud == null) _hud = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11, fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.78f, 0.78f, 0.84f, 1f) }
            };
            float x = 10, y = Screen.height - 74;
            string modeStr = Mode switch
            { GizmoMode.Rotate=>"Rotate (E)", GizmoMode.Scale=>"Scale (R)", _=>"Translate (W)" };
            GUI.Label(new Rect(x, y,    240, 18), $"Mode: {modeStr}", _hud);
            GUI.Label(new Rect(x, y+18, 240, 18), $"Space: {(IsLocal?"Local":"World")}  (Q)", _hud);
            if (Selected != null)
                GUI.Label(new Rect(x, y+36, 240, 18), $"Selected: {Selected.displayName}", _hud);
            GUI.Label(new Rect(x, y+54, 280, 18), "W E R = mode  |  Q = space  |  Del = delete", _hud);
        }
    }
}