using Camera.Presenter.ViewModels;
using Loxodon.Framework.Contexts;
using MapEditor.Presenter.View;
using MapEditor.Presenter.ViewModels;
using SceneEditor.Presenter.View;
using SceneEditor.Presenter.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;
using UnityEngine.UIElements;

namespace EditorShell.Presenter.View
{
    public class SideBarController : MonoBehaviour
    {
        [SerializeField] private Light     _directionalLight;
        [SerializeField] private Material  _gridMaterial;

        private TransformGizmoView     _gizmo;
        private SceneObjectSpawnerView _spawner;

        // Shader property IDs — rename to match your grid shader's property names if needed
        private static readonly int _propGridThickness = Shader.PropertyToID("_GridThickness");
        private static readonly int _propBgColor       = Shader.PropertyToID("_BackgroundColor");
        private static readonly int _propGridColor     = Shader.PropertyToID("_GridColor");
        private static readonly int _propGridOpacity   = Shader.PropertyToID("_GridOpacity");
        private static readonly int _propTransSides    = Shader.PropertyToID("_TransparentSides");

        // --- Camera ---
        public Slider     OrbitSensitivity { get; private set; }
        public Slider     ZoomSpeed        { get; private set; }
        public Slider     PanSensitivity   { get; private set; }
        public Slider     OrbitSmoothing   { get; private set; }
        public Slider     ZoomSmoothing    { get; private set; }
        public Slider     MinPitch         { get; private set; }
        public Slider     MaxPitch         { get; private set; }
        public Slider     MinZoom          { get; private set; }
        public Slider     MaxZoom          { get; private set; }
        public Button     BtnResetView     { get; private set; }

        // --- Directional Light ---
        public Slider     LightIntensity   { get; private set; }
        public Slider     LightColorR      { get; private set; }
        public Slider     LightColorG      { get; private set; }
        public Slider     LightColorB      { get; private set; }
        public Slider     LightPitch       { get; private set; }
        public Slider     LightYaw         { get; private set; }
        public Slider     LightShadows     { get; private set; }
        public Slider     AmbientIntensity { get; private set; }
        public Slider     AmbientColorR    { get; private set; }
        public Slider     AmbientColorG    { get; private set; }
        public Slider     AmbientColorB    { get; private set; }

        // --- Terrain & Grid ---
        public Button       BtnSaveMap       { get; private set; }
        public Button       BtnLoadMap       { get; private set; }
        public Slider       TerrainColorR    { get; private set; }
        public Slider       TerrainColorG    { get; private set; }
        public Slider       TerrainColorB    { get; private set; }
        public IntegerField TerrainWidth     { get; private set; }
        public IntegerField TerrainDepth     { get; private set; }
        public IntegerField TerrainThickness { get; private set; }
        public Button       BtnRegenerateMap { get; private set; }
        public Slider       GridCellSizeX    { get; private set; }
        public Slider       GridCellSizeY    { get; private set; }
        public Slider       GridThickness    { get; private set; }
        public Slider       GridBgR          { get; private set; }
        public Slider       GridBgG          { get; private set; }
        public Slider       GridBgB          { get; private set; }
        public Slider       GridColorR       { get; private set; }
        public Slider       GridColorG       { get; private set; }
        public Slider       GridColorB       { get; private set; }
        public Slider       GridOpacity      { get; private set; }
        public Toggle       TransparentSides { get; private set; }

        // --- Outliner ---
        public TextField      OutlinerSearch    { get; private set; }
        public Action<ObjectViewModel> OnObjectSelected;
        private VisualElement _outlinerList;

        // --- Toggle ---
        private Button        _btnToggle;
        private VisualElement _scroll;
        private VisualElement _sideBar;
        private bool          _isExpanded = true;
        private const float   ExpandedWidth = 350f;

        private IPanel _uiPanel;
        private MapEditorViewModel _vm;

        public void Init(VisualElement root)
        {
            _uiPanel = root.panel;

            _gizmo   = FindFirstObjectByType<TransformGizmoView>();
            _spawner = FindFirstObjectByType<SceneObjectSpawnerView>();

            if (_gizmo == null)
                Debug.LogWarning("[SideBarController] TransformGizmoView not found. Add it to Main Camera.");
            if (_spawner == null)
                Debug.LogWarning("[SideBarController] SceneObjectSpawnerView not found in scene.");

            _sideBar   = root.Q<VisualElement>("side-bar");
            _btnToggle = root.Q<Button>("btn-sidebar-toggle");
            _scroll    = root.Q<ScrollView>("side-bar-scroll");
            _btnToggle.clicked += ToggleSidebar;

            BindCameraElements(root);
            BindLightElements(root);
            BindTerrainGridElements(root);
            BindOutlinerElements(root);

            _vm = Context.GetApplicationContext()
                         .GetContainer()
                         .Resolve<MapEditorViewModel>();
            if (_vm == null) return;

            ConnectCamera();
            ConnectLight();
            ConnectTerrainGrid();
            ConnectOutliner();
        }

        // ── UI element queries ────────────────────────────────────────────────

        private void BindCameraElements(VisualElement root)
        {
            OrbitSensitivity = root.Q<Slider>("slider-orbit-sensitivity");
            ZoomSpeed        = root.Q<Slider>("slider-zoom-speed");
            PanSensitivity   = root.Q<Slider>("slider-pan-sensitivity");
            OrbitSmoothing   = root.Q<Slider>("slider-orbit-smoothing");
            ZoomSmoothing    = root.Q<Slider>("slider-zoom-smoothing");
            MinPitch         = root.Q<Slider>("slider-min-pitch");
            MaxPitch         = root.Q<Slider>("slider-max-pitch");
            MinZoom          = root.Q<Slider>("slider-min-zoom");
            MaxZoom          = root.Q<Slider>("slider-max-zoom");
            BtnResetView     = root.Q<Button>("btn-reset-view");
        }

        private void BindLightElements(VisualElement root)
        {
            LightIntensity   = root.Q<Slider>("slider-light-intensity");
            LightColorR      = root.Q<Slider>("slider-light-color-r");
            LightColorG      = root.Q<Slider>("slider-light-color-g");
            LightColorB      = root.Q<Slider>("slider-light-color-b");
            LightPitch       = root.Q<Slider>("slider-light-pitch");
            LightYaw         = root.Q<Slider>("slider-light-yaw");
            LightShadows     = root.Q<Slider>("slider-light-shadows");
            AmbientIntensity = root.Q<Slider>("slider-ambient-intensity");
            AmbientColorR    = root.Q<Slider>("slider-ambient-color-r");
            AmbientColorG    = root.Q<Slider>("slider-ambient-color-g");
            AmbientColorB    = root.Q<Slider>("slider-ambient-color-b");
        }

        private void BindTerrainGridElements(VisualElement root)
        {
            BtnSaveMap       = root.Q<Button>("btn-save-map");
            BtnLoadMap       = root.Q<Button>("btn-load-map");
            TerrainColorR    = root.Q<Slider>("slider-terrain-color-r");
            TerrainColorG    = root.Q<Slider>("slider-terrain-color-g");
            TerrainColorB    = root.Q<Slider>("slider-terrain-color-b");
            TerrainWidth     = root.Q<IntegerField>("field-terrain-width");
            TerrainDepth     = root.Q<IntegerField>("field-terrain-depth");
            TerrainThickness = root.Q<IntegerField>("field-terrain-thickness");
            BtnRegenerateMap = root.Q<Button>("btn-regenerate-map");
            GridCellSizeX    = root.Q<Slider>("slider-grid-cell-size-x");
            GridCellSizeY    = root.Q<Slider>("slider-grid-cell-size-y");
            GridThickness    = root.Q<Slider>("slider-grid-thickness");
            GridBgR          = root.Q<Slider>("slider-grid-bg-r");
            GridBgG          = root.Q<Slider>("slider-grid-bg-g");
            GridBgB          = root.Q<Slider>("slider-grid-bg-b");
            GridColorR       = root.Q<Slider>("slider-grid-color-r");
            GridColorG       = root.Q<Slider>("slider-grid-color-g");
            GridColorB       = root.Q<Slider>("slider-grid-color-b");
            GridOpacity      = root.Q<Slider>("slider-grid-opacity");
            TransparentSides = root.Q<Toggle>("toggle-transparent-sides");
        }

        private void BindOutlinerElements(VisualElement root)
        {
            OutlinerSearch = root.Q<TextField>("outliner-search");
            _outlinerList  = root.Q<VisualElement>("outliner-list");
            OutlinerSearch.RegisterValueChangedCallback(e => FilterOutliner(e.newValue));
        }

        // ── Scene click-to-select ─────────────────────────────────────────────

        private void Update()
        {
            if (!Input.GetMouseButtonDown(0)) return;
            if (_spawner == null || _vm == null) return;
            if (_gizmo != null && _gizmo.IsInteractingWithGizmo) return;
            if (IsPointerOverUI()) return;

            var cam = UnityEngine.Camera.main;
            if (cam == null) return;

            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
            {
                if (_spawner.TryGetId(hit.collider.gameObject, out string id))
                {
                    foreach (ObjectViewModel obj in _vm.Map.Objects)
                    {
                        if (obj.Model.Id == id) { SelectObject(obj); return; }
                    }
                }
            }

            // Click on empty space → deselect
            if (_gizmo != null) _gizmo.Deselect();
        }

        private bool IsPointerOverUI()
        {
            if (_uiPanel == null) return false;
            var screen = Input.mousePosition;
            var panelPos = RuntimePanelUtils.ScreenToPanel(
                _uiPanel, new Vector2(screen.x, Screen.height - screen.y));
            return _uiPanel.Pick(panelPos) != null;
        }

        // ── System connections ────────────────────────────────────────────────

        private void ConnectCamera()
        {
            CameraViewModel cam      = _vm.Camera;
            var             settings = cam.Model.Settings;

            // initialize sliders from current state
            OrbitSensitivity.SetValueWithoutNotify(cam.OrbitSensitivity.Value);
            ZoomSpeed.SetValueWithoutNotify(cam.ZoomSpeed.Value);
            PanSensitivity.SetValueWithoutNotify(cam.PanSensitivity.Value);
            OrbitSmoothing.SetValueWithoutNotify(settings.OrbitSmoothing);
            ZoomSmoothing.SetValueWithoutNotify(settings.ZoomSmoothing);
            MinPitch.SetValueWithoutNotify(cam.MinPitch.Value);
            MaxPitch.SetValueWithoutNotify(cam.MaxPitch.Value);
            MinZoom.SetValueWithoutNotify(cam.MinZoomDistance.Value);
            MaxZoom.SetValueWithoutNotify(cam.MaxZoomDistance.Value);

            // slider → VM
            OrbitSensitivity.RegisterValueChangedCallback(e => cam.OrbitSensitivity.Value  = e.newValue);
            ZoomSpeed.RegisterValueChangedCallback(e        => cam.ZoomSpeed.Value         = e.newValue);
            PanSensitivity.RegisterValueChangedCallback(e   => cam.PanSensitivity.Value    = e.newValue);
            OrbitSmoothing.RegisterValueChangedCallback(e   => settings.OrbitSmoothing     = e.newValue);
            ZoomSmoothing.RegisterValueChangedCallback(e    => settings.ZoomSmoothing      = e.newValue);
            MinPitch.RegisterValueChangedCallback(e         => cam.MinPitch.Value          = e.newValue);
            MaxPitch.RegisterValueChangedCallback(e         => cam.MaxPitch.Value          = e.newValue);
            MinZoom.RegisterValueChangedCallback(e          => cam.MinZoomDistance.Value   = e.newValue);
            MaxZoom.RegisterValueChangedCallback(e          => cam.MaxZoomDistance.Value   = e.newValue);
            BtnResetView.clicked += cam.Reset;

            // VM → slider (keeps UI in sync if values change externally)
            cam.OrbitSensitivity.ValueChanged += (_, __) => OrbitSensitivity.SetValueWithoutNotify(cam.OrbitSensitivity.Value);
            cam.ZoomSpeed.ValueChanged        += (_, __) => ZoomSpeed.SetValueWithoutNotify(cam.ZoomSpeed.Value);
            cam.PanSensitivity.ValueChanged   += (_, __) => PanSensitivity.SetValueWithoutNotify(cam.PanSensitivity.Value);
            cam.MinPitch.ValueChanged         += (_, __) => MinPitch.SetValueWithoutNotify(cam.MinPitch.Value);
            cam.MaxPitch.ValueChanged         += (_, __) => MaxPitch.SetValueWithoutNotify(cam.MaxPitch.Value);
            cam.MinZoomDistance.ValueChanged  += (_, __) => MinZoom.SetValueWithoutNotify(cam.MinZoomDistance.Value);
            cam.MaxZoomDistance.ValueChanged  += (_, __) => MaxZoom.SetValueWithoutNotify(cam.MaxZoomDistance.Value);
        }

        private void ConnectLight()
        {
            if (_directionalLight == null) return;

            // initialize sliders from scene state
            LightIntensity.SetValueWithoutNotify(_directionalLight.intensity);
            LightColorR.SetValueWithoutNotify(_directionalLight.color.r);
            LightColorG.SetValueWithoutNotify(_directionalLight.color.g);
            LightColorB.SetValueWithoutNotify(_directionalLight.color.b);
            Vector3 euler = _directionalLight.transform.eulerAngles;
            LightPitch.SetValueWithoutNotify(euler.x);
            LightYaw.SetValueWithoutNotify(euler.y);
            LightShadows.SetValueWithoutNotify(_directionalLight.shadowStrength);
            AmbientIntensity.SetValueWithoutNotify(RenderSettings.ambientIntensity);
            AmbientColorR.SetValueWithoutNotify(RenderSettings.ambientLight.r);
            AmbientColorG.SetValueWithoutNotify(RenderSettings.ambientLight.g);
            AmbientColorB.SetValueWithoutNotify(RenderSettings.ambientLight.b);

            // slider → scene
            LightIntensity.RegisterValueChangedCallback(e => _directionalLight.intensity      = e.newValue);
            LightColorR.RegisterValueChangedCallback(_    => ApplyLightColor());
            LightColorG.RegisterValueChangedCallback(_    => ApplyLightColor());
            LightColorB.RegisterValueChangedCallback(_    => ApplyLightColor());
            LightPitch.RegisterValueChangedCallback(_     => ApplyLightRotation());
            LightYaw.RegisterValueChangedCallback(_       => ApplyLightRotation());
            LightShadows.RegisterValueChangedCallback(e   => _directionalLight.shadowStrength = e.newValue);
            AmbientIntensity.RegisterValueChangedCallback(e => RenderSettings.ambientIntensity = e.newValue);
            AmbientColorR.RegisterValueChangedCallback(_ => ApplyAmbientColor());
            AmbientColorG.RegisterValueChangedCallback(_ => ApplyAmbientColor());
            AmbientColorB.RegisterValueChangedCallback(_ => ApplyAmbientColor());
        }

        private void ConnectTerrainGrid()
        {
            TerrainLayoutViewModel terrain = _vm.Map.Terrain;
            if (terrain != null)
            {
                // initialize from VM
                TerrainWidth.SetValueWithoutNotify(terrain.Width.Value);
                TerrainDepth.SetValueWithoutNotify(terrain.Depth.Value);
                TerrainThickness.SetValueWithoutNotify(terrain.Thickness.Value);
                if (terrain.Model.Color != null && terrain.Model.Color.Length >= 3)
                {
                    TerrainColorR.SetValueWithoutNotify(terrain.Model.Color[0]);
                    TerrainColorG.SetValueWithoutNotify(terrain.Model.Color[1]);
                    TerrainColorB.SetValueWithoutNotify(terrain.Model.Color[2]);
                }

                // fields → VM
                TerrainWidth.RegisterValueChangedCallback(e     => terrain.Width.Value     = e.newValue);
                TerrainDepth.RegisterValueChangedCallback(e     => terrain.Depth.Value     = e.newValue);
                TerrainThickness.RegisterValueChangedCallback(e => terrain.Thickness.Value = e.newValue);

                // VM → fields
                terrain.Width.ValueChanged     += (_, __) => TerrainWidth.SetValueWithoutNotify(terrain.Width.Value);
                terrain.Depth.ValueChanged     += (_, __) => TerrainDepth.SetValueWithoutNotify(terrain.Depth.Value);
                terrain.Thickness.ValueChanged += (_, __) => TerrainThickness.SetValueWithoutNotify(terrain.Thickness.Value);

                // Regenerate: passes all current values including color through the use case (supports undo/redo)
                BtnRegenerateMap.clicked += () =>
                {
                    var color = new float[] { TerrainColorR.value, TerrainColorG.value, TerrainColorB.value, 1f };
                    _vm.GenerateTerrain.Execute(terrain.Model, TerrainWidth.value, TerrainDepth.value, TerrainThickness.value, terrain.Model.Height, color);
                };
            }

            BtnSaveMap.clicked += () => _vm.SaveMap.Execute(_vm.Map.Model);
            BtnLoadMap.clicked += () => _vm.LoadMap.Execute();

            // Grid cell size
            if (_vm.Grid != null)
            {
                GridCellSizeX.SetValueWithoutNotify(_vm.Grid.CellSize);
                GridCellSizeY.SetValueWithoutNotify(_vm.Grid.CellSize);
                GridCellSizeX.RegisterValueChangedCallback(e => _vm.Grid.CellSize = e.newValue);
                GridCellSizeY.RegisterValueChangedCallback(e => _vm.Grid.CellSize = e.newValue);
            }

            // Grid shader properties
            if (_gridMaterial != null)
            {
                GridThickness.SetValueWithoutNotify(_gridMaterial.GetFloat(_propGridThickness));
                Color bg   = _gridMaterial.GetColor(_propBgColor);
                Color grid = _gridMaterial.GetColor(_propGridColor);
                GridBgR.SetValueWithoutNotify(bg.r);
                GridBgG.SetValueWithoutNotify(bg.g);
                GridBgB.SetValueWithoutNotify(bg.b);
                GridColorR.SetValueWithoutNotify(grid.r);
                GridColorG.SetValueWithoutNotify(grid.g);
                GridColorB.SetValueWithoutNotify(grid.b);
                GridOpacity.SetValueWithoutNotify(_gridMaterial.GetFloat(_propGridOpacity));
                TransparentSides.SetValueWithoutNotify(_gridMaterial.GetFloat(_propTransSides) > 0.5f);

                GridThickness.RegisterValueChangedCallback(e => _gridMaterial.SetFloat(_propGridThickness, e.newValue));
                GridBgR.RegisterValueChangedCallback(_ => _gridMaterial.SetColor(_propBgColor,  new Color(GridBgR.value, GridBgG.value, GridBgB.value)));
                GridBgG.RegisterValueChangedCallback(_ => _gridMaterial.SetColor(_propBgColor,  new Color(GridBgR.value, GridBgG.value, GridBgB.value)));
                GridBgB.RegisterValueChangedCallback(_ => _gridMaterial.SetColor(_propBgColor,  new Color(GridBgR.value, GridBgG.value, GridBgB.value)));
                GridColorR.RegisterValueChangedCallback(_ => _gridMaterial.SetColor(_propGridColor, new Color(GridColorR.value, GridColorG.value, GridColorB.value)));
                GridColorG.RegisterValueChangedCallback(_ => _gridMaterial.SetColor(_propGridColor, new Color(GridColorR.value, GridColorG.value, GridColorB.value)));
                GridColorB.RegisterValueChangedCallback(_ => _gridMaterial.SetColor(_propGridColor, new Color(GridColorR.value, GridColorG.value, GridColorB.value)));
                GridOpacity.RegisterValueChangedCallback(e    => _gridMaterial.SetFloat(_propGridOpacity, e.newValue));
                TransparentSides.RegisterValueChangedCallback(e => _gridMaterial.SetFloat(_propTransSides, e.newValue ? 1f : 0f));
            }
        }

        private void ConnectOutliner()
        {
            foreach (ObjectViewModel obj in _vm.Map.Objects)
                AddOutlinerEntry(obj);

            _vm.Map.Objects.CollectionChanged += OnObjectsChanged;
        }

        private void OnObjectsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
                foreach (ObjectViewModel obj in e.NewItems)
                    AddOutlinerEntry(obj);
            else if (e.Action == NotifyCollectionChangedAction.Remove)
                foreach (ObjectViewModel obj in e.OldItems)
                    _outlinerList.Q(obj.Model.Id)?.RemoveFromHierarchy();
            else if (e.Action == NotifyCollectionChangedAction.Reset)
                _outlinerList.Clear();
        }

        private void AddOutlinerEntry(ObjectViewModel obj)
        {
            var row = new VisualElement { name = obj.Model.Id };
            row.style.flexDirection  = FlexDirection.Row;
            row.style.alignItems     = Align.Center;
            row.style.paddingTop     = row.style.paddingBottom = 2;
            row.style.paddingLeft    = row.style.paddingRight  = 4;

            var label = new Label(obj.DisplayName.Value);
            label.style.flexGrow = 1;
            obj.DisplayName.ValueChanged += (_, __) => label.text = obj.DisplayName.Value;

            var btnSelect = new Button(() => SelectObject(obj)) { text = "i" };
            btnSelect.style.width   = btnSelect.style.height  = 20;
            btnSelect.style.marginLeft = 2;

            var btnDelete = new Button(() => _vm.DeleteObject.Execute(obj.Model)) { text = "X" };
            btnDelete.style.width   = btnDelete.style.height  = 20;
            btnDelete.style.marginLeft = 2;
            btnDelete.style.backgroundColor = new StyleColor(new Color(0.78f, 0.24f, 0.24f));
            btnDelete.style.color           = new StyleColor(Color.white);

            row.Add(label);
            row.Add(btnSelect);
            row.Add(btnDelete);
            _outlinerList.Add(row);
        }

        // ── Selection ─────────────────────────────────────────────────────────

        private void SelectObject(ObjectViewModel obj)
        {
            if (_gizmo != null && _spawner != null &&
                _spawner.TryGetGameObject(obj.Model.Id, out GameObject go))
                _gizmo.Select(go, obj.Model);

            OnObjectSelected?.Invoke(obj);
        }

        // ── Light helpers ─────────────────────────────────────────────────────

        private void ApplyLightColor()
            => _directionalLight.color = new Color(LightColorR.value, LightColorG.value, LightColorB.value);

        private void ApplyLightRotation()
            => _directionalLight.transform.eulerAngles = new Vector3(LightPitch.value, LightYaw.value, 0f);

        private void ApplyAmbientColor()
            => RenderSettings.ambientLight = new Color(AmbientColorR.value, AmbientColorG.value, AmbientColorB.value);

        // ── Sidebar toggle ────────────────────────────────────────────────────

        private void ToggleSidebar()
        {
            _isExpanded            = !_isExpanded;
            _scroll.style.display  = _isExpanded ? DisplayStyle.Flex : DisplayStyle.None;
            _sideBar.style.width   = _isExpanded ? ExpandedWidth : _btnToggle.resolvedStyle.width + 4;
            _btnToggle.text        = _isExpanded ? "▶" : "▼";
        }

        // ── Outliner helpers ──────────────────────────────────────────────────

        public void SetOutlinerItems(IEnumerable<string> names)
        {
            _outlinerList.Clear();
            foreach (string name in names)
                AddOutlinerItem(name);
        }

        public void AddOutlinerItem(string itemName)
        {
            var label = new Label(itemName) { name = itemName };
            _outlinerList.Add(label);
        }

        public void RemoveOutlinerItem(string itemName)
        {
            _outlinerList.Q(itemName)?.RemoveFromHierarchy();
        }

        private void FilterOutliner(string query)
        {
            foreach (VisualElement child in _outlinerList.Children())
            {
                bool visible = string.IsNullOrEmpty(query)
                    || child.name.Contains(query, StringComparison.OrdinalIgnoreCase);
                child.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }
    }
}
