using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using VTT;
using VTT.Grid;

namespace VTT.UI
{
    // ── Prefab category definition ────────────────────────────────────────────
    [Serializable]
    public class PrefabCategory
    {
        public string          name    = "Category";
        public List<GameObject> prefabs = new();
    }

    /// <summary>
    /// Right-side resizable settings panel (OnGUI, zero dependencies).
    /// Folder-style prefab browser | Placement system integration |
    /// Camera settings | Light settings | Terrain & Grid settings.
    /// </summary>
    [AddComponentMenu("VTT/Panel UI")]
    public class VTTPanelUI : MonoBehaviour
    {
        // ── Inspector ─────────────────────────────────────────────────────────
        [Header("Scene References (auto-found if empty)")]
        [SerializeField] private CameraVTT      cameraVTT;
        [SerializeField] private Light          directionalLight;
        [SerializeField] private TerrainBuilder terrainBuilder;
        [SerializeField] private MeshRenderer   gridRenderer;
        [SerializeField] private GridInput      gridInput;

        [Header("Prefab Categories")]
        [SerializeField] private List<PrefabCategory> categories = new();

        /// <summary>Live category list — AssetImportManager adds to this at runtime.</summary>
        public List<PrefabCategory> Categories => categories;

        [Header("Prefab Previews")]
        [SerializeField] private int previewLayer = 31;

        [Header("Panel")]
        [SerializeField] private float panelWidth    = 280f;
        [SerializeField] private float minPanelWidth = 200f;
        [SerializeField] private float maxPanelWidth = 480f;

        // ── Static UI blocking (read by CameraVTT & GridInput) ────────────────
        public static bool IsMouseOverUI { get; private set; }

        // ── Shader IDs ────────────────────────────────────────────────────────
        private static readonly int SH_CellSize     = Shader.PropertyToID("_Cell_Size");
        private static readonly int SH_GridThick    = Shader.PropertyToID("_Grid_Thickness");
        private static readonly int SH_BgColor      = Shader.PropertyToID("_Background_Color");
        private static readonly int SH_GridColor    = Shader.PropertyToID("_Grid_Color");
        private static readonly int SH_Transparent  = Shader.PropertyToID("_Transparent");
        private static readonly int SH_GridOpacity  = Shader.PropertyToID("_Grid_Opacity");

        // ── Section open states ───────────────────────────────────────────────
        private bool _secCamera   = true;
        private bool _secLight    = false;
        private bool _secTerrain  = false;
        private bool _secPrefabs  = true;
        private bool _secOutliner = true;

        // ── Scroll ────────────────────────────────────────────────────────────
        private Vector2 _scroll;

        // ── Resize ────────────────────────────────────────────────────────────
        private bool _resizing;

        // ── Terrain cache ─────────────────────────────────────────────────────
        private Color  _terrainColor;
        private int    _terrainW, _terrainD, _terrainThick;
        private Material _gridMat;

        // ── Prefab browser ────────────────────────────────────────────────────
        private int          _openCategory = -1;   // -1 = root view
        private Texture2D[,] _previews;
        private bool[,]      _previewDone;

        // ── Placement state ───────────────────────────────────────────────────
        private bool       _placing;
        private GameObject _placingPrefab;
        private GameObject _placingPreview;
        private bool       _placingValid;
        private string     _currentPlacingCategory = "Uncategorized";

        // Panel visibility
        private bool    _panelVisible  = true;

        // Outliner
        private Vector2 _outlinerScroll;
        private string  _outlinerSearch = "";

        // Extra UI rect (e.g. gizmo panel) — blocks camera input
        private static Rect _extraUIRect;
        public  static void SetExtraUIRect(Rect r) => _extraUIRect = r;

        // ── Styles ────────────────────────────────────────────────────────────
        private GUIStyle _sPanel, _sHdr, _sHdrOpen, _sBody;
        private GUIStyle _sLabel, _sMuted, _sGroupLbl;
        private GUIStyle _sBtn, _sBtnSmall, _sBtnCancel;
        private GUIStyle _sField, _sFolder, _sCard;
        private bool     _stylesReady;

        // ── Colours ───────────────────────────────────────────────────────────
        private static readonly Color C_PANEL  = new(0.09f, 0.09f, 0.12f, 0.97f);
        private static readonly Color C_HDR    = new(0.14f, 0.14f, 0.19f, 1f);
        private static readonly Color C_BODY   = new(0.11f, 0.11f, 0.14f, 1f);
        private static readonly Color C_INPUT  = new(0.17f, 0.17f, 0.22f, 1f);
        private static readonly Color C_BTN    = new(0.20f, 0.18f, 0.38f, 1f);
        private static readonly Color C_CANCEL = new(0.38f, 0.12f, 0.12f, 1f);
        private static readonly Color C_FOLDER = new(0.16f, 0.16f, 0.22f, 1f);
        private static readonly Color C_CARD   = new(0.16f, 0.15f, 0.21f, 1f);
        private static readonly Color C_TEXT   = new(0.82f, 0.82f, 0.88f, 1f);
        private static readonly Color C_MUTED  = new(0.52f, 0.52f, 0.60f, 1f);
        private static readonly Color C_SEP    = new(0.24f, 0.24f, 0.32f, 0.6f);

        private static readonly Color A_CAM  = new(0.25f, 0.42f, 0.80f, 1f);
        private static readonly Color A_LITE = new(0.80f, 0.57f, 0.14f, 1f);
        private static readonly Color A_TER  = new(0.19f, 0.57f, 0.33f, 1f);
        private static readonly Color A_PRE  = new(0.55f, 0.25f, 0.72f, 1f);
        private static readonly Color A_OUT  = new(0.20f, 0.65f, 0.70f, 1f);

        // ── Lifecycle ─────────────────────────────────────────────────────────
        private void Start()
        {
            if (cameraVTT        == null) cameraVTT        = Camera.main?.GetComponent<CameraVTT>();
            if (directionalLight == null) directionalLight  = FindFirstObjectByType<Light>();
            if (terrainBuilder   == null) terrainBuilder    = FindFirstObjectByType<TerrainBuilder>();
            if (gridInput        == null) gridInput         = FindFirstObjectByType<GridInput>();
            if (gridRenderer     == null && terrainBuilder != null)
                gridRenderer = terrainBuilder.GetComponent<MeshRenderer>();

            if (terrainBuilder != null)
            {
                _terrainW     = terrainBuilder.width;
                _terrainD     = terrainBuilder.depth;
                _terrainThick = Mathf.RoundToInt(terrainBuilder.thickness);
                _terrainColor = terrainBuilder.terrainColor;
            }
            if (gridRenderer != null) _gridMat = gridRenderer.material;

            // Allocate preview arrays
            int catCount  = categories?.Count ?? 0;
            int maxPrefab = 0;
            if (categories != null)
                foreach (var c in categories)
                    if (c?.prefabs != null) maxPrefab = Mathf.Max(maxPrefab, c.prefabs.Count);

            _previews    = new Texture2D[catCount, maxPrefab];
            _previewDone = new bool[catCount, maxPrefab];
            for (int ci = 0; ci < catCount; ci++)
                if (categories[ci]?.prefabs != null)
                    for (int pi = 0; pi < categories[ci].prefabs.Count; pi++)
                        if (categories[ci].prefabs[pi] != null)
                            StartCoroutine(RenderPreview(ci, pi, categories[ci].prefabs[pi]));
        }

        private void Update()
        {
            if (!_placing) return;

            // Update preview ghost position — snap to actual terrain surface
            if (gridInput != null && gridInput.HoveredCell != null)
            {
                var cell = gridInput.HoveredCell;
                var ps   = PlacementSystem.Instance;

                Vector3 flatPos  = GridManager.Instance.GridToWorld(cell.X, cell.Z);
                Vector3 worldPos = SampleTerrainHeight(flatPos);

                if (_placingPreview != null)
                    _placingPreview.transform.position = worldPos;

                bool canPlace = ps != null && _placingPreview != null &&
                    _placingPreview.TryGetComponent<PlaceableObject>(out var po) &&
                    ps.CanPlace(po, new Vector2Int(cell.X, cell.Z));
                _placingValid = canPlace;
            }

            // Left-click: confirm
            if (Input.GetMouseButtonDown(0) && !IsMouseOverUI)
            {
                ConfirmPlacement();
                return;
            }

            // Right-click or Escape: cancel
            if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
                CancelPlacement();
        }

        /// <summary>
        /// Raycast straight down from high above (xz) to find the real terrain surface Y.
        /// Falls back to flatPos if nothing is hit.
        /// </summary>
        private Vector3 SampleTerrainHeight(Vector3 flatPos)
        {
            var origin = new Vector3(flatPos.x, 2000f, flatPos.z);
            // Use the same layer mask as GridInput so we only hit the terrain collider
            LayerMask mask = gridInput != null
                ? RF<LayerMask>(gridInput, "terrainLayer")
                : Physics.DefaultRaycastLayers;

            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 4000f, mask))
                return new Vector3(flatPos.x, hit.point.y, flatPos.z);

            return flatPos;
        }

        // ── OnGUI ─────────────────────────────────────────────────────────────
        private void OnGUI()
        {
            BuildStyles();

            float sw = Screen.width;
            float sh = Screen.height;

            // ── Toggle button — always visible top-right corner ───────────────
            float btnW = 32f, btnH = 28f;
            float btnX = _panelVisible ? sw - panelWidth - btnW - 2f : sw - btnW - 4f;
            float btnY = 4f;

            var toggleRect = new Rect(btnX, btnY, btnW, btnH);
            IsMouseOverUI = (_panelVisible && new Rect(sw - panelWidth, 0, panelWidth, sh)
                                .Contains(Event.current.mousePosition))
                         || toggleRect.Contains(Event.current.mousePosition)
                         || _extraUIRect.Contains(Event.current.mousePosition);
            _extraUIRect  = Rect.zero;  // reset each frame

            // Button background
            GUI.color = new Color(0.14f, 0.13f, 0.22f, 0.95f);
            GUI.DrawTexture(toggleRect, Texture2D.whiteTexture);
            GUI.color = Color.white;

            string arrow = _panelVisible ? "▶" : "◀";
            if (GUI.Button(toggleRect, arrow, new GUIStyle(_sMuted)
                { alignment = TextAnchor.MiddleCenter, fontSize = 13,
                  normal = { background = null }, hover = { background = null } }))
                _panelVisible = !_panelVisible;

            if (!_panelVisible) return;

            // ── Panel ─────────────────────────────────────────────────────────
            var panelRect = new Rect(sw - panelWidth, 0, panelWidth, sh);
            GUI.Box(panelRect, GUIContent.none, _sPanel);

            DrawResizeHandle(sw, sh);

            // Header
            GUI.color = new Color(0.07f, 0.07f, 0.09f, 1f);
            GUI.DrawTexture(new Rect(sw - panelWidth, 0, panelWidth, 36), Texture2D.whiteTexture);
            GUI.color = Color.white;
            GUI.Label(new Rect(sw - panelWidth + 12, 0, panelWidth - 100, 36),
                "VTT SETTINGS", _sGroupLbl);

            // Undo / Redo buttons in header
            var hist = CommandHistory.Instance;
            float hbw = 28f;
            GUI.enabled = hist != null && hist.CanUndo;
            if (GUI.Button(new Rect(sw - panelWidth + (_iw > 0 ? _iw : panelWidth - 16) - hbw * 2 - 10, 6, hbw, 24),
                "↩", new GUIStyle(_sBtnSmall) { fontSize = 13 }))
                hist?.Undo();
            GUI.enabled = hist != null && hist.CanRedo;
            if (GUI.Button(new Rect(sw - panelWidth + (_iw > 0 ? _iw : panelWidth - 16) - hbw - 6, 6, hbw, 24),
                "↪", new GUIStyle(_sBtnSmall) { fontSize = 13 }))
                hist?.Redo();
            GUI.enabled = true;

            // Accent line under header
            GUI.color = C_MUTED * 0.5f;
            GUI.DrawTexture(new Rect(sw - panelWidth, 35, panelWidth, 1), Texture2D.whiteTexture);
            GUI.color = Color.white;

            // Placement banner
            float bannerH = 0;
            if (_placing)
            {
                bannerH = 32f;
                DrawPlacementBanner(sw, sh);
            }

            // Scroll area
            float scrollTop = 36 + bannerH;
            var scrollArea  = new Rect(sw - panelWidth, scrollTop, panelWidth, sh - scrollTop);
            float innerW    = panelWidth - 14;

            _scroll = GUI.BeginScrollView(scrollArea, _scroll,
                new Rect(0, 0, innerW, _contentH), false, true);

            _cy  = 4;
            _iw  = innerW - 4;

            Section("  Camera",           A_CAM,  ref _secCamera,  DrawCamera);
            Section("  Directional Light",A_LITE, ref _secLight,   DrawLight);
            Section("  Terrain & Grid",   A_TER,  ref _secTerrain, DrawTerrain);
            Section("  Prefabs",          A_PRE,  ref _secPrefabs,  DrawPrefabs);
            Section("  Outliner",         A_OUT,  ref _secOutliner, DrawOutliner);

            _contentH = _cy + 20;
            GUI.EndScrollView();
        }

        // ── Layout state ──────────────────────────────────────────────────────
        private float _cy;
        private float _iw;
        private float _contentH = 800f;

        // ── Section ───────────────────────────────────────────────────────────
        // Cached body heights per section — keyed by title, updated each frame
        private readonly Dictionary<string, float> _sectionHeights = new();

        private void Section(string title, Color accent, ref bool open, Action drawBody)
        {
            // Header button
            var hRect = new Rect(0, _cy, _iw, 30);
            if (GUI.Button(hRect, GUIContent.none, open ? _sHdrOpen : _sHdr))
                open = !open;

            // Accent strip
            GUI.color = accent;
            GUI.DrawTexture(new Rect(0, _cy, 3, 30), Texture2D.whiteTexture);
            GUI.color = Color.white;

            GUI.Label(new Rect(10, _cy, _iw - 26, 30), title, _sLabel);
            GUI.Label(new Rect(_iw - 22, _cy, 18, 30), open ? "▼" : "▶", _sMuted);
            _cy += 30;

            if (!open) { _cy += 2; return; }

            float startY = _cy;

            // Draw background FIRST using last frame's cached height so controls
            // are never occluded by the background rect — fixes click/hover blocking
            _sectionHeights.TryGetValue(title, out float cachedH);
            if (cachedH > 0)
            {
                GUI.color = C_BODY;
                GUI.DrawTexture(new Rect(0, startY, _iw, cachedH), Texture2D.whiteTexture);
                GUI.color = Color.white;
            }

            // Draw controls once — they are always on top because background was first
            drawBody();

            // Cache the real height for next frame
            _sectionHeights[title] = _cy - startY;

            _cy += 4;
        }

        // ── CAMERA ────────────────────────────────────────────────────────────
        private void DrawCamera()
        {
            _cy += 6;
            if (cameraVTT == null) { Muted("CameraVTT not found."); return; }
            var c = cameraVTT;

            SliderF(c, "orbitSensitivity", "Orbit Sensitivity", 0.05f, 2f);
            SliderF(c, "zoomSpeed",        "Zoom Speed",         1f,   15f);
            SliderF(c, "panSensitivity",   "Pan Sensitivity",  0.005f, 0.15f);
            SliderF(c, "orbitSmoothing",   "Orbit Smoothing",    1f,   30f);
            SliderF(c, "zoomSmoothing",    "Zoom Smoothing",     1f,   20f);
            Sep();
            GroupLbl("Limits");
            SliderF(c, "minPolarAngle",   "Min Pitch",   0f,  45f);
            SliderF(c, "maxPolarAngle",   "Max Pitch",  46f,  89f);
            SliderF(c, "minZoomDistance", "Min Zoom",  0.5f,  20f);
            SliderF(c, "maxZoomDistance", "Max Zoom",   20f, 150f);
            Sep();
            if (Btn("Reset View  (F)"))
                c.FocusOn(RF<Vector3>(c, "initialPivot"), RF<float>(c, "initialDistance"));
            _cy += 8;
        }

        // ── LIGHT ─────────────────────────────────────────────────────────────
        private void DrawLight()
        {
            _cy += 6;
            if (directionalLight == null) { Muted("No Light found."); return; }
            var lt = directionalLight;

            FloatRow("Intensity", lt.intensity, 0f, 5f, v => lt.intensity = v);
            ColorRow("Color", lt.color, c => lt.color = c);
            Sep();
            GroupLbl("Direction");
            var e = lt.transform.eulerAngles;
            FloatRow("Pitch (X)", e.x, 0f, 180f, v => lt.transform.eulerAngles = new Vector3(v, e.y, 0));
            FloatRow("Yaw (Y)",   e.y, -180f, 180f, v => lt.transform.eulerAngles = new Vector3(e.x, v, 0));
            FloatRow("Shadows", lt.shadowStrength, 0f, 1f, v => lt.shadowStrength = v);
            Sep();
            GroupLbl("Ambient");
            FloatRow("Intensity", RenderSettings.ambientIntensity, 0f, 2f,
                v => RenderSettings.ambientIntensity = v);
            ColorRow("Ambient Color", RenderSettings.ambientLight,
                c => RenderSettings.ambientLight = c);
            _cy += 8;
        }

        // ── TERRAIN ───────────────────────────────────────────────────────────
        private void DrawTerrain()
        {
            _cy += 6;

            // ── Save / Load ───────────────────────────────────────────────────
            var sl = Persistence.MapSaveLoad.Instance;
            if (sl == null)
            {
                Muted("Add MapSaveLoad component to enable Save/Load.");
            }
            else
            {
                bool busy = sl.IsBusy;
                GUI.enabled = !busy;

                float hw = (_iw - 18) / 2f;
                if (GUI.Button(new Rect(8, _cy, hw, 26), busy ? "…" : "Save Map", _sBtn))
                    sl.SaveWithDialog();
                if (GUI.Button(new Rect(10 + hw, _cy, hw, 26), busy ? "…" : "Load Map", _sBtn))
                    sl.LoadWithDialog();
                GUI.enabled = true;
                _cy += 30;

                if (!string.IsNullOrEmpty(sl.Status))
                    Muted(sl.Status);
            }

            Sep();

            if (terrainBuilder != null)
            {
                GroupLbl("Terrain");
                var tb = terrainBuilder;
                ColorRow("Terrain Color", _terrainColor, c => { _terrainColor = c; tb.terrainColor = c; });
                IntRow("Width",     ref _terrainW);
                IntRow("Depth",     ref _terrainD);
                IntRow("Thickness", ref _terrainThick);
                if (Btn("Regenerate Map"))
                {
                    // Capture before state for undo
                    int   prevW  = tb.width, prevD = tb.depth;
                    int   prevTh = Mathf.RoundToInt(tb.thickness);
                    float prevY  = tb.baseHeight;
                    Color prevC  = tb.terrainColor;

                    tb.width        = Mathf.Max(1, _terrainW);
                    tb.depth        = Mathf.Max(1, _terrainD);
                    tb.thickness    = Mathf.Max(1, _terrainThick);
                    tb.terrainColor = _terrainColor;
                    tb.GenerateTerrain();

                    CommandHistory.Instance?.Record(new TerrainCommand(tb,
                        prevW, prevD, prevTh, prevY, prevC,
                        tb.width, tb.depth, Mathf.RoundToInt(tb.thickness),
                        tb.baseHeight, tb.terrainColor));
                }
                Sep();
            }

            if (_gridMat == null) { Muted("Grid material not found."); _cy += 8; return; }
            GroupLbl("Grid Shader");

            var cs = _gridMat.GetVector(SH_CellSize);
            FloatRow("Cell Size X", cs.x, 0.25f, 10f,
                v => _gridMat.SetVector(SH_CellSize, new Vector4(v, cs.y)));
            FloatRow("Cell Size Y", cs.y, 0.25f, 10f,
                v => _gridMat.SetVector(SH_CellSize, new Vector4(cs.x, v)));
            FloatRow("Grid Thickness", _gridMat.GetFloat(SH_GridThick), 0.005f, 0.5f,
                v => _gridMat.SetFloat(SH_GridThick, v));
            ColorRow("Background",  _gridMat.GetColor(SH_BgColor),
                c => _gridMat.SetColor(SH_BgColor, c));
            ColorRow("Grid Color",  _gridMat.GetColor(SH_GridColor),
                c => _gridMat.SetColor(SH_GridColor, c));
            FloatRow("Grid Opacity", _gridMat.GetFloat(SH_GridOpacity), 0f, 1f,
                v => _gridMat.SetFloat(SH_GridOpacity, v));
            ToggleRow("Transparent Sides", _gridMat.GetFloat(SH_Transparent) > 0.5f,
                v => _gridMat.SetFloat(SH_Transparent, v ? 1f : 0f));
            _cy += 8;
        }

        // ── PREFABS ───────────────────────────────────────────────────────────
        private void DrawPrefabs()
        {
            _cy += 6;

            // ── Import button ─────────────────────────────────────────────────
            var mgr = AssetImportManager.Instance;
            if (mgr == null)
            {
                // Component not in scene — show a hint instead of silently hiding the button
                Muted("Add AssetImportManager component to a GameObject to enable import.");
            }
            else
            {
                bool busy = mgr.IsImporting;
                GUI.enabled = !busy;
                if (Btn(busy ? "Importing…" : "Import glTF / GLB"))
                    mgr.ImportFromFileDialog();
                GUI.enabled = true;

                if (!string.IsNullOrEmpty(mgr.ImportStatus))
                    Muted(mgr.ImportStatus);
            }
            Sep();

            if (categories == null || categories.Count == 0)
            {
                Muted("No categories.\nAdd categories in the Inspector or import a model.");
                _cy += 8;
                return;
            }

            if (_openCategory < 0)
                DrawCategoryRoot();
            else
                DrawCategoryContents(_openCategory);
        }

        private void DrawCategoryRoot()
        {
            float cardW  = 78f, cardH  = 64f, gap = 6f, margin = 8f;
            float usable = _iw - margin * 2f;
            int   cols   = Mathf.Max(1, Mathf.FloorToInt((usable + gap) / (cardW + gap)));

            int row = 0, col = 0;
            for (int ci = 0; ci < categories.Count; ci++)
            {
                var cat = categories[ci];
                if (cat == null) continue;

                float cx = margin + col * (cardW + gap);
                float cy = _cy + row * (cardH + gap);

                // Folder button
                if (GUI.Button(new Rect(cx, cy, cardW, cardH), GUIContent.none, _sFolder))
                    _openCategory = ci;

                // Folder icon
                GUI.color = new Color(0.80f, 0.64f, 0.22f, 1f);
                GUI.Label(new Rect(cx + cardW * 0.5f - 14, cy + 6, 28, 28),
                    "▣", new GUIStyle(_sLabel) { fontSize = 22, alignment = TextAnchor.MiddleCenter });
                GUI.color = Color.white;

                // Count badge
                int cnt = cat.prefabs?.Count ?? 0;
                GUI.Label(new Rect(cx, cy + 36, cardW, 14),
                    $"{cnt} item{(cnt != 1 ? "s" : "")}", _sMuted);

                // Name
                GUI.Label(new Rect(cx + 2, cy + 46, cardW - 4, 14),
                    cat.name, new GUIStyle(_sMuted) { alignment = TextAnchor.UpperCenter, wordWrap = true });

                col++;
                if (col >= cols) { col = 0; row++; }
            }

            int totalRows = Mathf.CeilToInt((float)categories.Count / cols);
            _cy += totalRows * (cardH + gap) + gap + 6;
        }

        private void DrawCategoryContents(int catIdx)
        {
            var cat = categories[catIdx];

            // ← Back button
            if (GUI.Button(new Rect(6, _cy, 60, 22), "← Back", _sBtnSmall))
            {
                _openCategory = -1;
                return;
            }
            GUI.Label(new Rect(72, _cy, _iw - 78, 22), cat.name, _sLabel);
            _cy += 28;
            Sep();

            if (cat.prefabs == null || cat.prefabs.Count == 0)
            {
                Muted("No prefabs in this category."); _cy += 8; return;
            }

            float cardW = 78f, cardH = 110f, gap = 6f, margin = 8f;
            float usable = _iw - margin * 2f;
            int   cols   = Mathf.Max(1, Mathf.FloorToInt((usable + gap) / (cardW + gap)));

            int row = 0, col = 0;
            for (int pi = 0; pi < cat.prefabs.Count; pi++)
            {
                var pf = cat.prefabs[pi];
                if (pf == null) continue;

                float cx = margin + col * (cardW + gap);
                float cy = _cy   + row * (cardH + gap);

                // Card bg
                GUI.color = C_CARD;
                GUI.DrawTexture(new Rect(cx, cy, cardW, cardH), Texture2D.whiteTexture);
                GUI.color = Color.white;

                // Preview
                var previewRect = new Rect(cx + 2, cy + 4, cardW - 4, 68);
                if (catIdx < _previews.GetLength(0) && pi < _previews.GetLength(1)
                    && _previewDone[catIdx, pi] && _previews[catIdx, pi] != null)
                    GUI.DrawTexture(previewRect, _previews[catIdx, pi], ScaleMode.ScaleToFit);
                else
                {
                    GUI.color = new Color(0.20f, 0.20f, 0.26f, 1f);
                    GUI.DrawTexture(previewRect, Texture2D.whiteTexture);
                    GUI.color = Color.white;
                    GUI.Label(previewRect, "…", new GUIStyle(_sMuted) { alignment = TextAnchor.MiddleCenter });
                }

                // Name
                GUI.Label(new Rect(cx + 2, cy + 74, cardW - 4, 18),
                    pf.name, new GUIStyle(_sMuted) { alignment = TextAnchor.UpperCenter, fontSize = 9, wordWrap = true });

                // Place button
                var style    = (_placing && _placingPrefab == pf) ? _sBtnCancel : _sBtnSmall;
                string label = (_placing && _placingPrefab == pf) ? "Cancel" : "Place";
                if (GUI.Button(new Rect(cx + 4, cy + 92, cardW - 8, 16), label, style))
                {
                    if (_placing && _placingPrefab == pf)
                        CancelPlacement();
                    else
                        BeginPlacing(pf, cat.name);
                }

                col++;
                if (col >= cols) { col = 0; row++; }
            }

            int totalRows = Mathf.CeilToInt((float)cat.prefabs.Count / cols);
            _cy += totalRows * (cardH + gap) + gap + 6;
        }

        // ── Placement banner ──────────────────────────────────────────────────
        private void DrawPlacementBanner(float sw, float sh)
        {
            float bannerY = 36f;
            GUI.color = _placingValid
                ? new Color(0.12f, 0.35f, 0.18f, 0.95f)
                : new Color(0.35f, 0.14f, 0.12f, 0.95f);
            GUI.DrawTexture(new Rect(sw - panelWidth, bannerY, panelWidth, 32), Texture2D.whiteTexture);
            GUI.color = Color.white;

            string msg = _placingValid ? "Left-click to place" : "Can't place here";
            GUI.Label(new Rect(sw - panelWidth + 8, bannerY + 2, panelWidth - 70, 28),
                $"Placing: {(_placingPrefab != null ? _placingPrefab.name : "?")}  —  {msg}", _sMuted);

            if (GUI.Button(new Rect(sw - 58, bannerY + 5, 50, 22), "Cancel", _sBtnCancel))
                CancelPlacement();
        }

        // ── Placement logic ───────────────────────────────────────────────────
        private void BeginPlacing(GameObject prefab, string category = "Uncategorized")
        {
            _currentPlacingCategory = category;
            CancelPlacement();
            _placing        = true;
            _placingPrefab  = prefab;
            _placingPreview = Instantiate(prefab);
            _placingPreview.SetActive(true);  // activate — prefab may be inactive (imported asset)

            // Semi-transparent ghost — clone material so prefab asset is untouched
            foreach (var r in _placingPreview.GetComponentsInChildren<Renderer>())
            {
                var mat = new Material(r.sharedMaterial);
                mat.SetFloat("_Mode", 3);
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.EnableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = 3000;
                mat.color       = new Color(1f, 1f, 1f, 0.45f);
                r.material      = mat;
            }

            // PlaceableObject needed for CanPlace checks — do NOT call PlacementSystem.BeginPreview
            // (that would spawn a second preview object)
            var previewPo = _placingPreview.GetComponent<PlaceableObject>()
                         ?? _placingPreview.AddComponent<PlaceableObject>();
            previewPo.ComputeFootprint(GridManager.Instance != null ? GridManager.Instance.CellSize : 1f);
        }

        private void ConfirmPlacement()
        {
            if (!_placing || gridInput?.HoveredCell == null) return;
            var cell = gridInput.HoveredCell;

            // Spawn real instance at terrain surface height
            var flatPos  = GridManager.Instance.GridToWorld(cell.X, cell.Z);
            var worldPos = SampleTerrainHeight(flatPos);

            var inst = Instantiate(_placingPrefab, worldPos, _placingPrefab.transform.rotation);
            inst.SetActive(true);  // activate — prefab may be inactive (imported asset)
            var po   = inst.GetComponent<PlaceableObject>() ?? inst.AddComponent<PlaceableObject>();
            po.ComputeFootprint(GridManager.Instance != null ? GridManager.Instance.CellSize : 1f);

            // Tag as a DecorObject so it appears in Outliner and is selectable by Gizmo
            var decor             = inst.GetComponent<DecorObject>() ?? inst.AddComponent<DecorObject>();
            decor.displayName     = _placingPrefab.name;
            decor.prefabName      = _placingPrefab.name;
            decor.category        = _currentPlacingCategory;

            // Mark as imported if this prefab came from AssetImportManager
            var aim = AssetImportManager.Instance;
            if (aim != null && System.Linq.Enumerable.Contains(aim.ImportedAssets, _placingPrefab))
            {
                decor.isImported = true;
                decor.importPath = aim.GetImportPath(_placingPrefab);
            }

            // Register with grid (occupies cell, marks it as blocked for pathfinding)
            PlacementSystem.Instance?.Place(po, new Vector2Int(cell.X, cell.Z));

            // Record undo command
            CommandHistory.Instance?.Record(new PlaceCommand(inst, decor, po, new Vector2Int(cell.X, cell.Z)));

            CancelPlacement();
        }

        private void CancelPlacement()
        {
            _placing = false;
            if (_placingPreview != null) Destroy(_placingPreview);
            _placingPreview = null;
            _placingPrefab  = null;
        }


        // ── OUTLINER ──────────────────────────────────────────────────────────
        private void DrawOutliner()
        {
            _cy += 6;
            var all = DecorObject.All;

            // Search bar
            GUI.color = new Color(0.17f, 0.17f, 0.22f, 1f);
            GUI.DrawTexture(new Rect(8, _cy, _iw - 16, 22), Texture2D.whiteTexture);
            GUI.color = Color.white;
            _outlinerSearch = GUI.TextField(new Rect(8, _cy, _iw - 16, 22),
                _outlinerSearch, _sField);
            _cy += 26;

            if (all.Count == 0)
            {
                Muted("No objects placed yet.");
                _cy += 8;
                return;
            }

            // Filter
            string filter = _outlinerSearch.ToLower();
            var filtered  = new System.Collections.Generic.List<DecorObject>();
            foreach (var d in all)
                if (d != null && (string.IsNullOrEmpty(filter) ||
                    d.displayName.ToLower().Contains(filter) ||
                    d.category.ToLower().Contains(filter)))
                    filtered.Add(d);

            // Group by category
            var byCategory = new System.Collections.Generic.Dictionary<string,
                System.Collections.Generic.List<DecorObject>>();
            foreach (var d in filtered)
            {
                if (!byCategory.ContainsKey(d.category))
                    byCategory[d.category] = new();
                byCategory[d.category].Add(d);
            }

            foreach (var kv in byCategory)
            {
                // Category header
                GUI.color = new Color(0.18f, 0.18f, 0.24f, 1f);
                GUI.DrawTexture(new Rect(0, _cy, _iw, 20), Texture2D.whiteTexture);
                GUI.color = Color.white;
                GUI.Label(new Rect(8, _cy, _iw - 8, 20), kv.Key.ToUpper(), _sGroupLbl);
                _cy += 22;

                foreach (var d in kv.Value)
                {
                    if (d == null) continue;
                    bool isSelected = TransformGizmo.Selected == d;

                    // Row bg
                    GUI.color = isSelected
                        ? new Color(0.22f, 0.36f, 0.52f, 1f)
                        : new Color(0.13f, 0.13f, 0.17f, 1f);
                    GUI.DrawTexture(new Rect(0, _cy, _iw, 24), Texture2D.whiteTexture);
                    GUI.color = Color.white;

                    // Selection accent strip
                    if (isSelected)
                    {
                        GUI.color = new Color(0.40f, 0.76f, 1f, 1f);
                        GUI.DrawTexture(new Rect(0, _cy, 3, 24), Texture2D.whiteTexture);
                        GUI.color = Color.white;
                    }

                    // Clickable label
                    if (GUI.Button(new Rect(6, _cy, _iw - 60, 24), GUIContent.none,
                        new GUIStyle { normal = { background = null } }))
                    {
                        TransformGizmo.Select(d);
                        // Focus camera on object
                        cameraVTT?.FocusOn(d.transform.position);
                    }
                    GUI.Label(new Rect(10, _cy + 4, _iw - 64, 16), d.displayName,
                        isSelected ? _sLabel : _sMuted);

                    // Focus button
                    if (GUI.Button(new Rect(_iw - 56, _cy + 3, 26, 18), "⊙", _sBtnSmall))
                        cameraVTT?.FocusOn(d.transform.position);

                    // Delete button
                    if (GUI.Button(new Rect(_iw - 28, _cy + 3, 22, 18), "✕", _sBtnCancel))
                    {
                        if (TransformGizmo.Selected == d) TransformGizmo.Deselect();
                        d.Delete();
                        _cy += 24;
                        break; // list changed, stop iterating
                    }

                    _cy += 26;
                }
                _cy += 4;
            }

            if (filtered.Count == 0 && !string.IsNullOrEmpty(filter))
                Muted("No results.");

            _cy += 8;
        }

        // ── Control helpers ───────────────────────────────────────────────────
        private void SliderF(object obj, string field, string label, float min, float max)
        {
            float v = 0; try { v = RF<float>(obj, field); } catch { }
            FloatRow(label, v, min, max, nv => WF(obj, field, nv));
        }

        private void FloatRow(string label, float value, float min, float max, Action<float> set)
        {
            GUI.Label(new Rect(8,        _cy + 1, _iw * 0.58f - 8, 14), label, _sMuted);
            GUI.Label(new Rect(_iw * 0.58f, _cy + 1, _iw * 0.38f, 14),
                value.ToString("F2"),
                new GUIStyle(_sMuted) { alignment = TextAnchor.UpperRight });
            float nv = GUI.HorizontalSlider(new Rect(8, _cy + 16, _iw - 16, 12), value, min, max);
            if (!Mathf.Approximately(nv, value)) try { set(nv); } catch { }
            _cy += 32;
        }

        private void ColorRow(string label, Color value, Action<Color> set)
        {
            // Label + swatch
            GUI.Label(new Rect(8, _cy, _iw - 34, 16), label, _sMuted);
            GUI.color = value;
            GUI.DrawTexture(new Rect(_iw - 28, _cy, 22, 14), Texture2D.whiteTexture);
            GUI.color = Color.white;
            _cy += 18;

            // R G B sliders
            float r = ColorSlider("R", value.r, new Color(0.70f, 0.20f, 0.20f, 1f));
            float g = ColorSlider("G", value.g, new Color(0.20f, 0.65f, 0.20f, 1f));
            float b = ColorSlider("B", value.b, new Color(0.22f, 0.40f, 0.78f, 1f));
            var nc = new Color(r, g, b, value.a);
            if (nc != value) try { set(nc); } catch { }
        }

        private float ColorSlider(string ch, float v, Color tint)
        {
            GUI.Label(new Rect(8, _cy + 1, 12, 12),
                ch, new GUIStyle(_sMuted) { fontSize = 9 });
            GUI.color = tint;
            float nv = GUI.HorizontalSlider(new Rect(22, _cy + 3, _iw - 30, 10), v, 0f, 1f);
            GUI.color = Color.white;
            _cy += 14;
            return nv;
        }

        private void IntRow(string label, ref int value)
        {
            GUI.Label(new Rect(8, _cy + 3, _iw * 0.5f - 8, 16), label, _sMuted);
            string s = GUI.TextField(new Rect(_iw * 0.5f, _cy, _iw * 0.47f, 22),
                value.ToString(), _sField);
            if (int.TryParse(s, out int p)) value = p;
            _cy += 26;
        }

        private void ToggleRow(string label, bool value, Action<bool> set)
        {
            bool nv = GUI.Toggle(new Rect(8, _cy, 18, 18), value, GUIContent.none);
            GUI.Label(new Rect(28, _cy, _iw - 36, 18), label, _sMuted);
            if (nv != value) try { set(nv); } catch { }
            _cy += 22;
        }

        private bool Btn(string label)
        {
            bool c = GUI.Button(new Rect(8, _cy, _iw - 16, 26), label, _sBtn);
            _cy += 30;
            return c;
        }

        private void Sep()
        {
            GUI.color = C_SEP;
            GUI.DrawTexture(new Rect(8, _cy, _iw - 16, 1), Texture2D.whiteTexture);
            GUI.color = Color.white;
            _cy += 6;
        }

        private void GroupLbl(string text)
        {
            GUI.Label(new Rect(8, _cy, _iw - 16, 16), text.ToUpper(), _sGroupLbl);
            _cy += 18;
        }

        private void Muted(string text)
        {
            GUI.Label(new Rect(8, _cy, _iw - 16, 36), text,
                new GUIStyle(_sMuted) { wordWrap = true });
            _cy += 36;
        }

        // ── Resize handle ─────────────────────────────────────────────────────
        private void DrawResizeHandle(float sw, float sh)
        {
            var hRect = new Rect(sw - panelWidth - 4, 0, 8, sh);
            bool hover = hRect.Contains(Event.current.mousePosition);

            GUI.color = _resizing ? new Color(0.6f, 0.55f, 0.95f, 1f)
                      : hover     ? new Color(0.48f, 0.42f, 0.80f, 1f)
                                  : new Color(0.30f, 0.26f, 0.58f, 1f);
            GUI.DrawTexture(new Rect(sw - panelWidth - 2, 0, 4, sh), Texture2D.whiteTexture);
            GUI.color = Color.white;

            var e = Event.current;
            if (e.type == EventType.MouseDown && hRect.Contains(e.mousePosition))
            { _resizing = true; e.Use(); }
            if (_resizing && (e.type == EventType.MouseDrag || e.type == EventType.MouseMove))
            { panelWidth = Mathf.Clamp(sw - e.mousePosition.x, minPanelWidth, maxPanelWidth); e.Use(); }
            if (_resizing && e.type == EventType.MouseUp)
            { _resizing = false; e.Use(); }
        }

        // ── Preview renderer ──────────────────────────────────────────────────
        private IEnumerator RenderPreview(int ci, int pi, GameObject prefab)
        {
            yield return new WaitForEndOfFrame();

            var rt    = new RenderTexture(128, 128, 24);
            var camGO = new GameObject($"_PrevCam_{ci}_{pi}");
            var cam   = camGO.AddComponent<Camera>();
            cam.clearFlags      = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.16f, 0.16f, 0.21f, 1f);
            cam.targetTexture   = rt;
            cam.cullingMask     = 1 << previewLayer;
            cam.fieldOfView     = 28f;
            cam.nearClipPlane   = 0.01f;
            cam.farClipPlane    = 500f;

            var inst = Instantiate(prefab, new Vector3(0, 5000, 0), Quaternion.Euler(15f, -30f, 0f));
            inst.SetActive(true);  // activate for preview render — prefab may be inactive
            SetLayerRec(inst, previewLayer);

            var bounds = GetBounds(inst);
            float dist = Mathf.Max(bounds.size.magnitude * 1.7f, 0.5f);
            camGO.transform.position = bounds.center + new Vector3(0, dist * 0.28f, -dist);
            camGO.transform.LookAt(bounds.center);

            yield return new WaitForEndOfFrame();
            cam.Render();

            var tex = new Texture2D(128, 128, TextureFormat.RGB24, false);
            RenderTexture.active = rt;
            tex.ReadPixels(new Rect(0, 0, 128, 128), 0, 0);
            tex.Apply();
            RenderTexture.active = null;

            if (ci < _previews.GetLength(0) && pi < _previews.GetLength(1))
            {
                _previews[ci, pi]    = tex;
                _previewDone[ci, pi] = true;
            }

            Destroy(inst);
            Destroy(camGO);
            rt.Release();
        }

        // ── Style builder ─────────────────────────────────────────────────────
        private void BuildStyles()
        {
            if (_stylesReady) return;
            _stylesReady = true;

            _sPanel = Styled(C_PANEL);
            _sHdr   = Styled(C_HDR,  C_TEXT,  11, FontStyle.Bold,   new RectOffset(0,0,0,0), 30);
            _sHdrOpen = Styled(new Color(0.17f,0.17f,0.22f,1f), C_TEXT, 11, FontStyle.Bold, new RectOffset(0,0,0,0), 30);
            _sBody  = Styled(C_BODY);
            _sLabel = Styled(Color.clear, C_TEXT,  11, FontStyle.Normal, new RectOffset(0,0,0,0), 18);
            _sMuted = Styled(Color.clear, C_MUTED, 10, FontStyle.Normal, new RectOffset(0,0,0,0), 16);
            _sGroupLbl = Styled(Color.clear, C_MUTED, 9, FontStyle.Bold, new RectOffset(0,0,0,0), 16);
            _sBtn    = Styled(C_BTN,    C_TEXT,  10, FontStyle.Normal, new RectOffset(4,4,4,4), 26);
            _sBtnSmall = Styled(C_BTN,  C_TEXT,  9,  FontStyle.Normal, new RectOffset(3,3,3,3), 18);
            _sBtnCancel = Styled(C_CANCEL, C_TEXT, 9, FontStyle.Normal, new RectOffset(3,3,3,3), 18);
            _sField  = Styled(C_INPUT,  C_TEXT,  11, FontStyle.Normal, new RectOffset(4,4,4,4), 22);
            _sFolder = Styled(C_FOLDER, C_TEXT,  10, FontStyle.Normal, new RectOffset(4,4,4,4), 64);
            _sCard   = Styled(C_CARD);
        }

        private GUIStyle Styled(Color bg, Color? fg = null, int size = 11,
            FontStyle fs = FontStyle.Normal, RectOffset pad = null, int fixH = 0)
        {
            var s = new GUIStyle(GUI.skin.box);
            if (bg.a > 0)
            {
                var t = new Texture2D(1, 1); t.SetPixel(0, 0, bg); t.Apply();
                s.normal.background   = t;
                s.hover.background    = t;
                s.active.background   = t;
                s.focused.background  = t;
                s.onNormal.background = t;
            }
            else s.normal.background = null;

            s.normal.textColor = fg ?? C_TEXT;
            s.hover.textColor  = fg ?? C_TEXT;
            s.fontSize  = size;
            s.fontStyle = fs;
            s.border    = new RectOffset(0,0,0,0);
            s.padding   = pad ?? new RectOffset(0,0,0,0);
            s.alignment = TextAnchor.MiddleLeft;
            if (fixH > 0) s.fixedHeight = fixH;
            return s;
        }

        // ── Reflection helpers ────────────────────────────────────────────────
        private static T RF<T>(object o, string n)
        {
            var f = o.GetType().GetField(n,
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            return f != null ? (T)f.GetValue(o) : default;
        }
        private static void WF(object o, string n, object v)
        {
            o.GetType().GetField(n,
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public)
                ?.SetValue(o, v);
        }

        // ── Utilities ─────────────────────────────────────────────────────────
        private static void SetLayerRec(GameObject go, int layer)
        {
            go.layer = layer;
            foreach (Transform c in go.transform) SetLayerRec(c.gameObject, layer);
        }
        private static Bounds GetBounds(GameObject go)
        {
            var rs = go.GetComponentsInChildren<Renderer>();
            if (rs.Length == 0) return new Bounds(go.transform.position, Vector3.one * 0.5f);
            var b = rs[0].bounds;
            foreach (var r in rs) b.Encapsulate(r.bounds);
            return b;
        }
    }
}