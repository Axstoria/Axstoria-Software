using Camera.Presenter.ViewModels;
using Loxodon.Framework.Contexts;
using MapEditor.Domain;
using MapEditor.Presenter.View;
using MapEditor.Presenter.ViewModels;
using SceneEditor.Domain;
using SceneEditor.Presenter.View;
using SceneEditor.Presenter.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Specialized;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

namespace EditorShell.Presenter.View
{
    public class SideBarController : MonoBehaviour
    {
        [SerializeField] private Light     _directionalLight;

        private TransformGizmoView     _gizmo;
        private SceneObjectSpawnerView _spawner;

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
        public VisualElement LightColorSwatch { get; private set; }
        public Slider     LightPitch       { get; private set; }
        public Slider     LightYaw         { get; private set; }
        public Slider     LightShadows     { get; private set; }
        public Slider     AmbientIntensity { get; private set; }
        public VisualElement AmbientColorSwatch { get; private set; }

        // --- Terrain & Grid ---
        public VisualElement TerrainColorSwatch { get; private set; }
        public IntegerField TerrainWidth     { get; private set; }
        public IntegerField TerrainDepth     { get; private set; }
        public IntegerField TerrainThickness { get; private set; }
        public Button       BtnRegenerateMap { get; private set; }
        public Slider       GridCellSizeX    { get; private set; }
        public Slider       GridCellSizeY    { get; private set; }

        // --- Selected Object Transform ---
        public FloatField PosX   { get; private set; }
        public FloatField PosY   { get; private set; }
        public FloatField PosZ   { get; private set; }
        public FloatField RotX   { get; private set; }
        public FloatField RotY   { get; private set; }
        public FloatField RotZ   { get; private set; }
        public FloatField ScaleX { get; private set; }
        public FloatField ScaleY { get; private set; }
        public FloatField ScaleZ { get; private set; }
        private VisualElement _transformFields;
        private Label         _labelNoSelection;
        private Toggle        _toggleIsPawn;
        private ObjectViewModel _selectedObject;
        private EventHandler    _onSelectedTransformChanged;
        private EventHandler  _onMetadataChanged;
        private VisualElement _notesList;
        private Label         _labelNoNotes;
        private VisualElement _tagsList;
        private Label         _labelNoTags;
        private DropdownField _tagAssignDropdown;
        private Button        _btnAddTag;
        private List<TagViewModel> _tagChoices = new();
        private NotifyCollectionChangedEventHandler _onTagsChanged;
        private VisualElement _sheetsList;
        private Label         _labelNoSheets;

        // --- Selection ---
        public Action<ObjectViewModel> OnObjectSelected;

        private VisualElement _root;

        private IPanel _uiPanel;
        private MapEditorViewModel _vm;

        private VisualElement _outlinerPane;
        private ColorPickerOverlay _colorPickerOverlay;
        private Color _ambientColor;

        public void Init(VisualElement root)
        {
            _root         = root;
            _uiPanel      = root.panel;
            _outlinerPane = root.Q<VisualElement>("outliner-pane");

            _gizmo   = FindFirstObjectByType<TransformGizmoView>();
            _spawner = FindFirstObjectByType<SceneObjectSpawnerView>();

            if (_gizmo == null)
                Debug.LogWarning("[SideBarController] TransformGizmoView not found. Add it to Main Camera.");
            if (_spawner == null)
                Debug.LogWarning("[SideBarController] SceneObjectSpawnerView not found in scene.");

            // Shared by every color swatch in the panel (terrain, light, ambient, ...).
            _colorPickerOverlay = gameObject.AddComponent<ColorPickerOverlay>();

            BindCameraElements(root);
            BindLightElements(root);
            BindTerrainGridElements(root);
            BindSelectedTransformElements(root);

            VisualElement settingsPane = root.Q<VisualElement>("settings-pane");
            settingsPane?.Query<Slider>().ForEach(slider =>
            {
                slider.RegisterCallback<PointerDownEvent>(
                    _ => slider.AddToClassList("slider--dragging"), TrickleDown.TrickleDown);
                slider.RegisterCallback<PointerCaptureOutEvent>(
                    _ => slider.RemoveFromClassList("slider--dragging"));
            });

            _vm = Context.GetApplicationContext()
                         .GetContainer()
                         .Resolve<MapEditorViewModel>();
            if (_vm == null) return;

            ConnectCamera();
            ConnectLight();
            ConnectTerrainGrid();
            ConnectSelectedTransform();
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
            LightIntensity     = root.Q<Slider>("slider-light-intensity");
            LightColorSwatch   = root.Q<VisualElement>("light-color-swatch");
            LightPitch         = root.Q<Slider>("slider-light-pitch");
            LightYaw           = root.Q<Slider>("slider-light-yaw");
            LightShadows       = root.Q<Slider>("slider-light-shadows");
            AmbientIntensity   = root.Q<Slider>("slider-ambient-intensity");
            AmbientColorSwatch = root.Q<VisualElement>("ambient-color-swatch");
        }

        private void BindTerrainGridElements(VisualElement root)
        {
            TerrainColorSwatch = root.Q<VisualElement>("terrain-color-swatch");

            TerrainWidth     = root.Q<IntegerField>("field-terrain-width");
            TerrainDepth     = root.Q<IntegerField>("field-terrain-depth");
            TerrainThickness = root.Q<IntegerField>("field-terrain-thickness");
            BtnRegenerateMap = root.Q<Button>("btn-regenerate-map");
            GridCellSizeX    = root.Q<Slider>("slider-grid-cell-size-x");
            GridCellSizeY    = root.Q<Slider>("slider-grid-cell-size-y");
        }

        private void BindSelectedTransformElements(VisualElement root)
        {
            _labelNoSelection = root.Q<Label>("label-no-selection");
            _transformFields  = root.Q<VisualElement>("transform-fields");
            _toggleIsPawn     = root.Q<Toggle>("toggle-is-pawn");
            PosX   = root.Q<FloatField>("field-pos-x");
            PosY   = root.Q<FloatField>("field-pos-y");
            PosZ   = root.Q<FloatField>("field-pos-z");
            RotX   = root.Q<FloatField>("field-rot-x");
            RotY   = root.Q<FloatField>("field-rot-y");
            RotZ   = root.Q<FloatField>("field-rot-z");
            ScaleX = root.Q<FloatField>("field-scale-x");
            ScaleY = root.Q<FloatField>("field-scale-y");
            ScaleZ = root.Q<FloatField>("field-scale-z");
            _notesList = root.Q<VisualElement>("notes-list");
            _labelNoNotes = root.Q<Label>("label-no-notes");
            _tagsList      = root.Q<VisualElement>("tags-list");
            _labelNoTags   = root.Q<Label>("label-no-tags");
            _tagAssignDropdown = root.Q<DropdownField>("tag-assign-dropdown");
            _btnAddTag     = root.Q<Button>("btn-add-tag");
            _sheetsList    = root.Q<VisualElement>("sheets-list");
            _labelNoSheets = root.Q<Label>("label-no-sheets");
        }

        private void RefreshTagChoices()
        {
            _tagChoices = new List<TagViewModel>(_vm.Map.Tags);
            _tagAssignDropdown.choices = _tagChoices.Select(t => t.Name.Value).ToList();
            _tagAssignDropdown.index = _tagChoices.Count > 0 ? 0 : -1;
        }

        private void OnAddTagClicked()
        {
            if (_selectedObject == null || _tagAssignDropdown.index < 0) return;
            TagValue selectedTag = _tagChoices[_tagAssignDropdown.index].Model;

            bool alreadyAssigned = _selectedObject.Model.Metadata?.Any(e =>
                e.EntryValue is TagValue tv && tv.Id == selectedTag.Id) == true;
            if (!alreadyAssigned)
                _vm.SetObjectMetadata.Execute(_selectedObject.Model, "tag", new TagValue { Id = selectedTag.Id });
        }

        // ── Scene click-to-select ─────────────────────────────────────────────

        private void Update()
        {
            HandleRightClickDeselect();

            if (!Input.GetMouseButtonDown(0)) return;
            if (_spawner == null || _vm == null) return;
            if (_gizmo != null && _gizmo.IsInteractingWithGizmo) return;
            if (IsPointerOverUI()) return;

            var cam = UnityEngine.Camera.main;
            if (cam == null) return;

            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
            {
                Debug.Log($"[SideBarController] Raycast hit '{hit.collider.gameObject.name}'.");
                if (_spawner.TryGetId(hit.collider.gameObject, out string id))
                {
                    foreach (ObjectViewModel obj in _vm.Map.Objects)
                    {
                        if (obj.Model.Id != id) continue;
                        if (_vm.Permissions.CanInteract(_vm.Session.CurrentPlayer, obj.Model))
                            SelectObject(obj);
                        return;
                    }
                    Debug.Log($"[SideBarController] Id '{id}' not found in _vm.Map.Objects.");
                }
                else
                {
                    Debug.Log($"[SideBarController] '{hit.collider.gameObject.name}' is not a registered spawned object.");
                }
            }
            else
            {
                Debug.Log("[SideBarController] Raycast hit nothing.");
            }

            // Click on empty space → deselect
            bool isAimingPawnMove = _gizmo != null && _gizmo.IsSelectModeActive
                && _selectedObject != null && _selectedObject.Model.IsPawn;
            if (_gizmo != null && !isAimingPawnMove) _gizmo.Deselect();
        }

        private void HandleRightClickDeselect()
        {
            if (_gizmo == null) return;
            if (!Input.GetMouseButtonDown(1)) return;
            bool overOutliner = IsPointerOverOutliner();
            bool overOtherUI  = IsPointerOverUI() && !overOutliner;
            if (overOtherUI) return;

            _gizmo.Deselect();
        }

        private bool IsPointerOverOutliner()
            => UIPointerUtility.IsOverElement(_uiPanel, _outlinerPane, Input.mousePosition);

        private bool IsPointerOverUI()
            => UIPointerUtility.IsOverUI(_uiPanel, _root, Input.mousePosition);

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

            // RenderSettings.ambientLight (flat color) is only used in Flat mode — the scene
            // otherwise defaults to Skybox mode, which silently ignores the Ambient Color sliders.
            RenderSettings.ambientMode = AmbientMode.Flat;

            // initialize sliders from scene state
            LightIntensity.SetValueWithoutNotify(_directionalLight.intensity);
            RefreshLightSwatch(_directionalLight.color);
            Vector3 euler = _directionalLight.transform.eulerAngles;
            LightPitch.SetValueWithoutNotify(euler.x);
            LightYaw.SetValueWithoutNotify(euler.y);
            LightShadows.SetValueWithoutNotify(_directionalLight.shadowStrength);
            AmbientIntensity.SetValueWithoutNotify(RenderSettings.ambientIntensity);
            _ambientColor = RenderSettings.ambientLight;
            RefreshAmbientSwatch(_ambientColor);

            // slider → scene (and mirrored into the domain LightSettings so it survives save/load)
            LightIntensity.RegisterValueChangedCallback(e =>
            {
                _directionalLight.intensity = e.newValue;
                SyncLightSettingsToDomain();
            });
            LightColorSwatch.RegisterCallback<ClickEvent>(_ =>
            {
                _colorPickerOverlay.Open(_directionalLight.color, LightColorSwatch, color =>
                {
                    _directionalLight.color = color;
                    RefreshLightSwatch(color);
                    SyncLightSettingsToDomain();
                });
            });
            LightPitch.RegisterValueChangedCallback(_ => { ApplyLightRotation(); SyncLightSettingsToDomain(); });
            LightYaw.RegisterValueChangedCallback(_   => { ApplyLightRotation(); SyncLightSettingsToDomain(); });
            LightShadows.RegisterValueChangedCallback(e =>
            {
                _directionalLight.shadowStrength = e.newValue;
                SyncLightSettingsToDomain();
            });
            // Flat ambient mode ignores RenderSettings.ambientIntensity, so intensity is folded into the color instead.
            AmbientIntensity.RegisterValueChangedCallback(_ => { ApplyAmbientColor(); SyncLightSettingsToDomain(); });
            AmbientColorSwatch.RegisterCallback<ClickEvent>(_ =>
            {
                _colorPickerOverlay.Open(_ambientColor, AmbientColorSwatch, color =>
                {
                    _ambientColor = color;
                    RefreshAmbientSwatch(color);
                    ApplyAmbientColor();
                    SyncLightSettingsToDomain();
                });
            });

            // Seed the domain LightSettings from the scene's initial state so a Save taken
            // before touching any slider still persists something meaningful.
            SyncLightSettingsToDomain();
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
                RefreshTerrainSwatch(terrain.Color.Value);

                // Width/Depth/Thickness only stage into the fields — the map itself isn't rebuilt
                // until "Regenerate Map" is clicked. Clamp here so typed values can't exceed the limits.
                TerrainWidth.RegisterValueChangedCallback(_     => ClampIntegerField(TerrainWidth, 1, 100));
                TerrainDepth.RegisterValueChangedCallback(_     => ClampIntegerField(TerrainDepth, 1, 100));
                TerrainThickness.RegisterValueChangedCallback(_ => ClampIntegerField(TerrainThickness, 1, 10));

                // Opens the real HSVPicker popup; live-updates the terrain mesh as soon as a color is picked
                TerrainColorSwatch.RegisterCallback<ClickEvent>(_ =>
                {
                    _colorPickerOverlay.Open(terrain.Color.Value, TerrainColorSwatch,
                        color => terrain.Color.Value = color);
                });

                // VM → fields
                terrain.Width.ValueChanged     += (_, __) => TerrainWidth.SetValueWithoutNotify(terrain.Width.Value);
                terrain.Depth.ValueChanged     += (_, __) => TerrainDepth.SetValueWithoutNotify(terrain.Depth.Value);
                terrain.Thickness.ValueChanged += (_, __) => TerrainThickness.SetValueWithoutNotify(terrain.Thickness.Value);
                terrain.Color.ValueChanged     += (_, __) => RefreshTerrainSwatch(terrain.Color.Value);

                // Regenerate: passes all current values including color through the use case (supports undo/redo)
                BtnRegenerateMap.clicked += () =>
                {
                    Color c = terrain.Color.Value;
                    var color = new float[] { c.r, c.g, c.b, 1f };
                    _vm.GenerateTerrain.Execute(terrain.Model, TerrainWidth.value, TerrainDepth.value, TerrainThickness.value, terrain.Model.Height, color);

                    // GenerateTerrainCommand writes straight to the domain model, bypassing the
                    // ViewModel's ObservableProperty setters — resync so TerrainBuilderView's
                    // ValueChanged-driven rebuild actually fires.
                    terrain.Width.Value     = terrain.Model.Width;
                    terrain.Depth.Value     = terrain.Model.Depth;
                    terrain.Thickness.Value = terrain.Model.Thickness;
                };
            }

            // Grid cell size
            if (_vm.Grid != null)
            {
                GridCellSizeX.SetValueWithoutNotify(_vm.Grid.CellSize);
                GridCellSizeY.SetValueWithoutNotify(_vm.Grid.CellSize);
                GridCellSizeX.RegisterValueChangedCallback(e => _vm.Grid.CellSize = e.newValue);
                GridCellSizeY.RegisterValueChangedCallback(e => _vm.Grid.CellSize = e.newValue);
            }
        }

        private void ConnectSelectedTransform()
        {
            PosX.RegisterValueChangedCallback(_   => ApplyTransformFromFields());
            PosY.RegisterValueChangedCallback(_   => ApplyTransformFromFields());
            PosZ.RegisterValueChangedCallback(_   => ApplyTransformFromFields());
            RotX.RegisterValueChangedCallback(_   => ApplyTransformFromFields());
            RotY.RegisterValueChangedCallback(_   => ApplyTransformFromFields());
            RotZ.RegisterValueChangedCallback(_   => ApplyTransformFromFields());
            ScaleX.RegisterValueChangedCallback(_ => ApplyTransformFromFields());
            ScaleY.RegisterValueChangedCallback(_ => ApplyTransformFromFields());
            ScaleZ.RegisterValueChangedCallback(_ => ApplyTransformFromFields());

            _vm.Map.Objects.CollectionChanged += OnSelectedObjectRemoved;
            if (_gizmo != null) _gizmo.OnSelectionChanged += OnGizmoSelectionChanged;

            RefreshTagChoices();
            _onTagsChanged = (_, __) => RefreshTagChoices();
            _vm.Map.Tags.CollectionChanged += _onTagsChanged;
            _btnAddTag.clicked += OnAddTagClicked;

            _toggleIsPawn.RegisterValueChangedCallback(e =>
            {
                if (_selectedObject != null) _selectedObject.IsPawn.Value = e.newValue;
            });

            _vm.Session.OnCurrentPlayerChanged += OnCurrentPlayerChanged;

            SetSelectedObject(null);
        }

        private void OnCurrentPlayerChanged()
        {
            if (_selectedObject == null) return;
            if (_vm.Permissions.CanInteract(_vm.Session.CurrentPlayer, _selectedObject.Model)) return;

            if (_gizmo != null) _gizmo.Deselect();
            SetSelectedObject(null);
        }

        private void OnGizmoSelectionChanged(SceneObject model)
        {
            if (model == null) { SetSelectedObject(null); return; }
            foreach (ObjectViewModel obj in _vm.Map.Objects)
            {
                if (obj.Model.Id == model.Id) { SetSelectedObject(obj); return; }
            }
            SetSelectedObject(null);
        }

        private void OnSelectedObjectRemoved(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action != NotifyCollectionChangedAction.Remove || _selectedObject == null) return;
            foreach (ObjectViewModel obj in e.OldItems)
            {
                if (obj.Model.Id == _selectedObject.Model.Id)
                {
                    SetSelectedObject(null);
                    return;
                }
            }
        }

        private void SetSelectedObject(ObjectViewModel obj)
        {
            if (_onSelectedTransformChanged != null && _selectedObject != null)
                _selectedObject.Model.OnTransformChanged -= _onSelectedTransformChanged;

            if (_onMetadataChanged != null && _selectedObject != null)
                _selectedObject.Model.OnMetadataChanged -= _onMetadataChanged;

            _selectedObject = obj;

            if (obj == null)
            {
                _labelNoSelection.style.display = DisplayStyle.Flex;
                _transformFields.style.display  = DisplayStyle.None;
                return;
            }

            _labelNoSelection.style.display = DisplayStyle.None;
            _transformFields.style.display  = DisplayStyle.Flex;

            _toggleIsPawn.SetValueWithoutNotify(obj.IsPawn.Value);

            _onSelectedTransformChanged = (_, __) => RefreshTransformFields();
            obj.Model.OnTransformChanged += _onSelectedTransformChanged;
            RefreshTransformFields();

            _onMetadataChanged = (_, __) => RefreshMetadata();
            obj.Model.OnMetadataChanged += _onMetadataChanged;
            RefreshMetadata();
        }

        private void RefreshTransformFields()
        {
            if (_selectedObject?.Model.Transform == null) return;
            TransformModel t = _selectedObject.Model.Transform;

            PosX.SetValueWithoutNotify(t.Position.x);
            PosY.SetValueWithoutNotify(t.Position.y);
            PosZ.SetValueWithoutNotify(t.Position.z);

            Vector3 euler = t.Rotation.eulerAngles;
            RotX.SetValueWithoutNotify(euler.x);
            RotY.SetValueWithoutNotify(euler.y);
            RotZ.SetValueWithoutNotify(euler.z);

            ScaleX.SetValueWithoutNotify(t.Scale.x);
            ScaleY.SetValueWithoutNotify(t.Scale.y);
            ScaleZ.SetValueWithoutNotify(t.Scale.z);
        }

        private void RefreshMetadata()
        {
            if (_notesList == null || _tagsList == null || _sheetsList == null) return;

            _notesList.Clear();
            _tagsList.Clear();
            _sheetsList.Clear();

            var entries = _selectedObject?.Model?.Metadata;

            var notes  = entries?.Where(e => e.EntryValue is NoteValue).ToList();
            var tags   = entries?.Where(e => e.EntryValue is TagValue).ToList();
            var sheets = entries?.Where(e => e.EntryValue is SheetValue).ToList();

            _labelNoNotes.style.display  = notes  is { Count: > 0 } ? DisplayStyle.None : DisplayStyle.Flex;
            _labelNoTags.style.display   = tags   is { Count: > 0 } ? DisplayStyle.None : DisplayStyle.Flex;
            _labelNoSheets.style.display = sheets is { Count: > 0 } ? DisplayStyle.None : DisplayStyle.Flex;

            if (notes != null)
            {
                foreach (var entry in notes)
                {
                    var label = new Label(FormatMetadataEntry(entry));
                    label.AddToClassList("settings-caption");
                    _notesList.Add(label);
                }
            }

            if (tags != null)
            {
                foreach (var entry in tags)
                {
                    var tagValue = (TagValue)entry.EntryValue;
                    var tagDef = _vm.Map.Tags.FirstOrDefault(t => t.Model.Id == tagValue.Id);

                    var chip = new VisualElement();
                    chip.AddToClassList("tag-chip");

                    var swatch = new VisualElement();
                    swatch.AddToClassList("tag-swatch");
                    string hex = tagDef?.HexColor.Value ?? "#808080";
                    if (ColorUtility.TryParseHtmlString(hex, out Color c))
                        swatch.style.backgroundColor = c;

                    var label = new Label(tagDef?.Name.Value ?? "(unknown tag)");
                    label.AddToClassList("tag-chip-name");

                    var remove = new Button { text = "×" };
                    remove.AddToClassList("tag-chip-remove");
                    remove.RegisterCallback<ClickEvent>(evt =>
                    {
                        evt.StopPropagation();
                        _vm.RemoveObjectMetadata.Execute(_selectedObject.Model, entry);
                    });

                    chip.Add(swatch);
                    chip.Add(label);
                    chip.Add(remove);
                    _tagsList.Add(chip);
                }
            }

            if (sheets != null)
            {
                foreach (var entry in sheets)
                {
                    var label = new Label(FormatMetadataEntry(entry));
                    label.AddToClassList("settings-caption");
                    _sheetsList.Add(label);
                }
            }
        }

        private static string FormatMetadataEntry(MetadataEntry entry) => entry.EntryValue switch
        {
            NoteValue  note => note.Text,
            SheetValue _    => "(not yet implemented)",
            _               => $"Unknown ({entry.EntryType})"
        };

        private void ApplyTransformFromFields()
        {
            if (_selectedObject == null || _vm == null) return;

            var newTransform = new TransformModel
            {
                Position = new Vector3(PosX.value, PosY.value, PosZ.value),
                Rotation = Quaternion.Euler(RotX.value, RotY.value, RotZ.value),
                Scale    = new Vector3(ScaleX.value, ScaleY.value, ScaleZ.value)
            };

            _vm.TransformObject.Execute(_selectedObject.Model, newTransform,
                $"Transform {_selectedObject.DisplayName.Value}");
        }

        // ── Real-time transform display during gizmo drag ────────────────────

        private void LateUpdate()
        {
            if (_selectedObject == null || _spawner == null) return;
            if (_gizmo == null || !_gizmo.IsInteractingWithGizmo) return;
            if (!_spawner.TryGetGameObject(_selectedObject.Model.Id, out GameObject go)) return;

            Transform t = go.transform;
            PosX.SetValueWithoutNotify(t.position.x);
            PosY.SetValueWithoutNotify(t.position.y);
            PosZ.SetValueWithoutNotify(t.position.z);

            Vector3 euler = t.eulerAngles;
            RotX.SetValueWithoutNotify(euler.x);
            RotY.SetValueWithoutNotify(euler.y);
            RotZ.SetValueWithoutNotify(euler.z);

            ScaleX.SetValueWithoutNotify(t.localScale.x);
            ScaleY.SetValueWithoutNotify(t.localScale.y);
            ScaleZ.SetValueWithoutNotify(t.localScale.z);
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

        private void ApplyLightRotation()
            => _directionalLight.transform.eulerAngles = new Vector3(LightPitch.value, LightYaw.value, 0f);

        private void ApplyAmbientColor()
            => RenderSettings.ambientLight = _ambientColor * AmbientIntensity.value;

        private void RefreshLightSwatch(Color color)
            => LightColorSwatch.style.backgroundColor = color;

        private void RefreshAmbientSwatch(Color color)
            => AmbientColorSwatch.style.backgroundColor = color;

        // Mirrors the live scene light/ambient state into the domain LightSettings so it's
        // actually there to serialize on Save (previously write-only/unused by this UI).
        private void SyncLightSettingsToDomain()
        {
            if (_directionalLight == null || _vm?.Map?.Model == null) return;

            LightSettings settings = _vm.Map.Model.LightSettings;
            settings.Intensity        = _directionalLight.intensity;
            settings.ColorR           = _directionalLight.color.r;
            settings.ColorG           = _directionalLight.color.g;
            settings.ColorB           = _directionalLight.color.b;
            Vector3 euler             = _directionalLight.transform.eulerAngles;
            settings.Pitch            = euler.x;
            settings.Yaw              = euler.y;
            settings.ShadowStrength   = _directionalLight.shadowStrength;
            settings.AmbientIntensity = AmbientIntensity.value;
            settings.AmbientColorR    = _ambientColor.r;
            settings.AmbientColorG    = _ambientColor.g;
            settings.AmbientColorB    = _ambientColor.b;
        }

        // Pushes loaded LightSettings onto the live scene and refreshes the sliders/swatches
        // to match. Called by LayoutUIManager after a map import.
        public void ApplyLightSettings(LightSettings settings)
        {
            if (_directionalLight == null || settings == null) return;

            _directionalLight.intensity = settings.Intensity;
            _directionalLight.color = new Color(settings.ColorR, settings.ColorG, settings.ColorB);
            _directionalLight.transform.eulerAngles = new Vector3(settings.Pitch, settings.Yaw, 0f);
            _directionalLight.shadowStrength = settings.ShadowStrength;

            _ambientColor = new Color(settings.AmbientColorR, settings.AmbientColorG, settings.AmbientColorB);
            AmbientIntensity.SetValueWithoutNotify(settings.AmbientIntensity);
            ApplyAmbientColor();

            LightIntensity.SetValueWithoutNotify(settings.Intensity);
            RefreshLightSwatch(_directionalLight.color);
            LightPitch.SetValueWithoutNotify(settings.Pitch);
            LightYaw.SetValueWithoutNotify(settings.Yaw);
            LightShadows.SetValueWithoutNotify(settings.ShadowStrength);
            RefreshAmbientSwatch(_ambientColor);
        }

        // ── Terrain helpers ───────────────────────────────────────────────────

        private void RefreshTerrainSwatch(Color color)
            => TerrainColorSwatch.style.backgroundColor = color;

        private static void ClampIntegerField(IntegerField field, int min, int max)
        {
            int clamped = Mathf.Clamp(field.value, min, max);
            if (clamped != field.value) field.SetValueWithoutNotify(clamped);
        }
    }
}
