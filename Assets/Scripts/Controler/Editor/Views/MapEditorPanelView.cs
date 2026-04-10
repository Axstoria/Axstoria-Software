using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Controler.Editor.ViewModels;
using Domain;
using Loxodon.Framework.Contexts;
using UnityEngine;
using UnityEngine.UIElements;

namespace Controler.Editor.Views
{
    [Serializable]
    public class PrefabCategory
    {
        public string          name    = "Category";
        public List<GameObject> prefabs = new();
    }

    /// <summary>
    /// UI Toolkit panel view — mirrors VTTPanelUI but wired to the MVVM architecture.
    /// Requires a UIDocument with SettingsPanel.uxml on the same GameObject.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class MapEditorPanelView : MonoBehaviour
    {
        [Header("Prefab Categories")]
        [SerializeField] private List<PrefabCategory> categories = new();

        [Header("Placement")]
        [SerializeField] private PlacementPreviewView placementPreview;

        private MapEditorViewModel _vm;
        private VisualElement      _root;

        // Stored handlers for clean unsubscription in OnDestroy
        private EventHandler                        _onCanUndoChanged;
        private EventHandler                        _onCanRedoChanged;
        private NotifyCollectionChangedEventHandler _onObjectsChanged;
        private TerrainLayoutViewModel              _subscribedTerrain;
        private EventHandler                        _onWidthChanged;
        private EventHandler                        _onDepthChanged;
        private EventHandler                        _onThicknessChanged;
        private EventHandler                        _onHeightChanged;

        private void Start()
        {
            _vm = Context.GetApplicationContext().GetContainer().Resolve<MapEditorViewModel>();
            if (_vm == null)
            {
                Debug.LogError("[MapEditorPanelView] MapEditorViewModel not registered. " +
                               "Ensure MapEditorBootstrapper runs before this script " +
                               "(Edit > Project Settings > Script Execution Order).");
                enabled = false;
                return;
            }

            _root = GetComponent<UIDocument>().rootVisualElement;

            BindHeader();
            BindTerrain();
            BindPrefabs();
            BindOutliner();

            _onCanUndoChanged = (_, __) => UpdateUndoRedo();
            _onCanRedoChanged = (_, __) => UpdateUndoRedo();
            _onObjectsChanged = (_, __) => RefreshOutliner();
            _vm.CanUndo.ValueChanged          += _onCanUndoChanged;
            _vm.CanRedo.ValueChanged          += _onCanRedoChanged;
            _vm.Map.Objects.CollectionChanged += _onObjectsChanged;
        }

        private void OnDestroy()
        {
            if (_vm == null) return;
            _vm.CanUndo.ValueChanged          -= _onCanUndoChanged;
            _vm.CanRedo.ValueChanged          -= _onCanRedoChanged;
            _vm.Map.Objects.CollectionChanged -= _onObjectsChanged;

            if (_subscribedTerrain != null)
            {
                _subscribedTerrain.Width.ValueChanged     -= _onWidthChanged;
                _subscribedTerrain.Depth.ValueChanged     -= _onDepthChanged;
                _subscribedTerrain.Thickness.ValueChanged -= _onThicknessChanged;
                _subscribedTerrain.Height.ValueChanged    -= _onHeightChanged;
            }
        }

        // ── Header ────────────────────────────────────────────────────────────

        private void BindHeader()
        {
            _root.Q<Button>("undo-btn").clicked += () => _vm.Undo();
            _root.Q<Button>("redo-btn").clicked += () => _vm.Redo();
            UpdateUndoRedo();
        }

        private void UpdateUndoRedo()
        {
            var undoBtn = _root.Q<Button>("undo-btn");
            var redoBtn = _root.Q<Button>("redo-btn");
            undoBtn?.SetEnabled(_vm.CanUndo.Value);
            redoBtn?.SetEnabled(_vm.CanRedo.Value);

            if (undoBtn != null)
                undoBtn.tooltip = _vm.CanUndo.Value ? _vm.UndoLabel.Value : "Nothing to undo";
        }

        // ── Terrain ───────────────────────────────────────────────────────────

        private void BindTerrain()
        {
            var terrain = _vm.Map.Terrain;
            if (terrain == null) return;

            var widthField     = _root.Q<IntegerField>("terrain-width");
            var depthField     = _root.Q<IntegerField>("terrain-depth");
            var thicknessField = _root.Q<IntegerField>("terrain-thickness");
            var heightField    = _root.Q<FloatField>("terrain-height");

            if (widthField     != null) widthField.value     = terrain.Width.Value;
            if (depthField     != null) depthField.value     = terrain.Depth.Value;
            if (thicknessField != null) thicknessField.value = terrain.Thickness.Value;
            if (heightField    != null) heightField.value    = terrain.Height.Value;

            // Sync incoming ViewModel changes to fields (e.g. from undo)
            _subscribedTerrain  = terrain;
            _onWidthChanged     = (_, __) => widthField?.SetValueWithoutNotify(terrain.Width.Value);
            _onDepthChanged     = (_, __) => depthField?.SetValueWithoutNotify(terrain.Depth.Value);
            _onThicknessChanged = (_, __) => thicknessField?.SetValueWithoutNotify(terrain.Thickness.Value);
            _onHeightChanged    = (_, __) => heightField?.SetValueWithoutNotify(terrain.Height.Value);
            terrain.Width.ValueChanged     += _onWidthChanged;
            terrain.Depth.ValueChanged     += _onDepthChanged;
            terrain.Thickness.ValueChanged += _onThicknessChanged;
            terrain.Height.ValueChanged    += _onHeightChanged;

            _root.Q<Button>("save-map-btn")?.RegisterCallback<ClickEvent>(_ =>
                _vm.SaveMap.Execute(_vm.Map.Model));

            _root.Q<Button>("load-map-btn")?.RegisterCallback<ClickEvent>(_ =>
                _vm.LoadMap.Execute());

            _root.Q<Button>("regen-btn")?.RegisterCallback<ClickEvent>(_ =>
            {
                int   w   = widthField?.value     ?? terrain.Width.Value;
                int   d   = depthField?.value     ?? terrain.Depth.Value;
                int   th  = thicknessField?.value ?? terrain.Thickness.Value;
                float h   = heightField?.value    ?? terrain.Height.Value;
                float[]col = terrain.Model.Color;
                _vm.GenerateTerrain.Execute(terrain.Model, w, d, th, h, col);
            });
        }

        // ── Prefabs ───────────────────────────────────────────────────────────

        private void BindPrefabs()
        {
            _root.Q<Button>("import-btn")?.RegisterCallback<ClickEvent>(_ =>
            {
                var status = _root.Q<Label>("import-status");
                var obj    = _vm.ImportAsset.Execute();
                if (status != null)
                    status.text = obj != null ? $"Imported: {obj.DisplayName}" : "Import cancelled";
            });

            var categoryList = _root.Q<VisualElement>("category-list");
            if (categoryList == null || categories == null) return;

            foreach (var cat in categories)
            {
                if (cat == null || cat.prefabs == null || cat.prefabs.Count == 0) continue;
                BuildCategorySection(categoryList, cat);
            }
        }

        private void BuildCategorySection(VisualElement parent, PrefabCategory cat)
        {
            // Category header (toggle)
            var header   = new VisualElement(); header.AddToClassList("category-header");
            var label    = new Label(cat.name); header.Add(label);
            var chevron  = new Label("▾");     chevron.AddToClassList("category-chevron"); header.Add(chevron);
            parent.Add(header);

            // Prefab grid (initially visible)
            var grid = new VisualElement(); grid.AddToClassList("prefab-grid");
            parent.Add(grid);

            foreach (var prefab in cat.prefabs)
            {
                if (prefab == null) continue;
                var item      = new VisualElement(); item.AddToClassList("prefab-item");
                var itemLabel = new Label(prefab.name); item.Add(itemLabel);

                var capturedPrefab = prefab;
                var capturedCat    = cat.name;

                item.RegisterCallback<ClickEvent>(_ =>
                {
                    if (placementPreview == null) return;
                    var domainObj = new SceneObject
                    {
                        Id          = System.Guid.NewGuid().ToString(),
                        DisplayName = capturedPrefab.name,
                        Category    = capturedCat,
                        IsImported  = false
                    };
                    placementPreview.BeginPlacement(capturedPrefab, domainObj);
                });

                grid.Add(item);
            }

            // Toggle grid on header click
            header.RegisterCallback<ClickEvent>(_ =>
            {
                bool visible = grid.style.display != DisplayStyle.None;
                grid.style.display = visible ? DisplayStyle.None : DisplayStyle.Flex;
                chevron.text = visible ? "▸" : "▾";
            });
        }

        // ── Outliner ──────────────────────────────────────────────────────────

        private void BindOutliner()
        {
            RefreshOutliner();
        }

        private void RefreshOutliner()
        {
            var list = _root.Q<ListView>("outliner-list");
            if (list == null) return;

            var objects = _vm.Map.Objects;

            list.makeItem = () =>
            {
                var row     = new VisualElement(); row.AddToClassList("outliner-item");
                var name    = new Label();         name.AddToClassList("item-name");
                var cat     = new Label();         cat.AddToClassList("item-category");
                var del     = new Button { text = "×" }; del.AddToClassList("delete-btn");
                row.Add(name); row.Add(cat); row.Add(del);
                return row;
            };

            list.bindItem = (element, i) =>
            {
                if (i >= objects.Count) return;
                var obj  = objects[i];
                var name = element.Q<Label>(className: "item-name");
                var cat  = element.Q<Label>(className: "item-category");
                var del  = element.Q<Button>(className: "delete-btn");

                if (name != null) name.text = string.IsNullOrEmpty(obj.DisplayName.Value) ? "(unnamed)" : obj.DisplayName.Value;
                if (cat  != null) cat.text  = obj.Category.Value ?? "";
                if (del  != null)
                {
                    del.clicked -= del.userData as Action;
                    Action onClick = () => _vm.DeleteObject.Execute(obj.Model);
                    del.userData   = onClick;
                    del.clicked   += onClick;
                }
            };

            list.itemsSource = new List<ObjectViewModel>(objects);
            list.Rebuild();
        }
    }
}
