using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Loxodon.Framework.Contexts;
using MapEditor.Presenter.ViewModels;
using SceneEditor.Domain;
using SceneEditor.Presenter;
using SceneEditor.Presenter.View;
using UnityEngine;
using UnityEngine.UIElements;

namespace EditorShell.Presenter.View
{
    [Serializable]
    public class PrefabEntry
    {
        public GameObject prefab;
        public string displayName;
    }

    [Serializable]
    public class BrowserCategory
    {
        public string name = "Category";
        public List<PrefabEntry> entries = new();
    }

    public class PrefabBrowserView : MonoBehaviour
    {
        private const string TabClass = "prefab-browser__tab";
        private const string TabSelectedClass = "prefab-browser__tab--selected";
        private const string ItemSelectedClass = "prefab-browser__item--selected";

        [Header("UI")]
        [SerializeField] private VisualTreeAsset _prefabBrowserUxml;

        [Header("Placement")]
        [SerializeField] private PlacementPreviewView _placementPreview;

        [Header("Prefab Categories")]
        [SerializeField] private List<BrowserCategory> _categories = new();

        private MapEditorViewModel _vm;
        private VisualElement _tabsContainer;
        private ScrollView _grid;

        private readonly Dictionary<string, Button> _tabs = new();
        private string _activeCategory;
        private VisualElement _selectedItem;

        private GameObject _armedPrefab;
        private string _armedCategory;
        private bool _rearmPending;
        private NotifyCollectionChangedEventHandler _onObjectsChanged;
        private EventHandler _onPlacementModeChanged;

        public void Init(VisualElement root)
        {
            if (_prefabBrowserUxml == null)
            {
                Debug.LogError($"[PrefabBrowserView] PrefabBrowser UXML asset not assigned on GameObject '{gameObject.name}'.", this);
                return;
            }

            _vm = Context.GetApplicationContext().GetContainer().Resolve<MapEditorViewModel>();
            if (_vm == null)
            {
                Debug.LogError("[PrefabBrowserView] MapEditorViewModel not registered. Ensure MapEditorBootstrapper runs first.");
                return;
            }

            VisualElement bottomContent = root.Q<VisualElement>("bottom-panel-content");
            if (bottomContent == null)
            {
                Debug.LogError("[PrefabBrowserView] 'bottom-panel-content' not found in root.");
                return;
            }

            VisualElement instance = _prefabBrowserUxml.Instantiate();
            instance.style.flexGrow = 1;
            bottomContent.Add(instance);

            _tabsContainer = instance.Q<VisualElement>("prefab-browser-tabs");
            _grid = instance.Q<ScrollView>("prefab-browser-grid");

            BuildTabs();
            PopulateRegistryCatalog();

            if (_categories.Count > 0 && _categories[0] != null)
                SelectCategory(_categories[0].name);

            _onObjectsChanged = OnObjectsChanged;
            _vm.Map.Objects.CollectionChanged += _onObjectsChanged;

            _onPlacementModeChanged = OnPlacementModeChanged;
            _vm.IsPlacementMode.ValueChanged += _onPlacementModeChanged;
        }

        private void OnDestroy()
        {
            if (_vm == null) return;
            if (_onObjectsChanged != null) _vm.Map.Objects.CollectionChanged -= _onObjectsChanged;
            if (_onPlacementModeChanged != null) _vm.IsPlacementMode.ValueChanged -= _onPlacementModeChanged;
        }

        private void OnObjectsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action != NotifyCollectionChangedAction.Add) return;
            if (_armedPrefab == null || _selectedItem == null) return;
            _rearmPending = true;
        }

        private void OnPlacementModeChanged(object sender, EventArgs e)
        {
            if (_vm.IsPlacementMode.Value) return;
            if (!_rearmPending) return;
            _rearmPending = false;
            if (_armedPrefab != null)
                StartPlacement(_armedPrefab, _armedCategory);
        }

        private void PopulateRegistryCatalog()
        {
            foreach (BrowserCategory cat in _categories)
            {
                if (cat == null || cat.entries == null) continue;
                foreach (PrefabEntry entry in cat.entries)
                {
                    if (entry == null || entry.prefab == null) continue;
                    ScenePrefabRegistry.RegisterCatalog(cat.name, GetDisplayName(entry), entry.prefab);
                }
            }
        }

        private void BuildTabs()
        {
            foreach (BrowserCategory cat in _categories)
            {
                if (cat == null || string.IsNullOrEmpty(cat.name)) continue;
                Button tab = new Button { text = cat.name };
                tab.AddToClassList(TabClass);
                string capturedName = cat.name;
                tab.clicked += () => SelectCategory(capturedName);
                _tabsContainer.Add(tab);
                _tabs[cat.name] = tab;
            }
        }

        private void SelectCategory(string categoryName)
        {
            _activeCategory = categoryName;

            foreach (KeyValuePair<string, Button> kvp in _tabs)
                kvp.Value.RemoveFromClassList(TabSelectedClass);

            if (_tabs.TryGetValue(categoryName, out Button selected))
                selected.AddToClassList(TabSelectedClass);

            BuildGrid(categoryName);
        }

        private void BuildGrid(string categoryName)
        {
            _grid.Clear();
            _selectedItem = null;

            BrowserCategory category = _categories.Find(c => c != null && c.name == categoryName);
            if (category == null || category.entries == null) return;

            foreach (PrefabEntry entry in category.entries)
            {
                if (entry == null || entry.prefab == null) continue;
                _grid.Add(BuildItem(entry, categoryName));
            }
        }

        private VisualElement BuildItem(PrefabEntry entry, string categoryName)
        {
            VisualElement item = new VisualElement();
            item.AddToClassList("prefab-browser__item");

            VisualElement thumbnail = new VisualElement();
            thumbnail.AddToClassList("prefab-browser__item-thumbnail");
            SetupThumbnail(thumbnail, entry);
            item.Add(thumbnail);

            Label label = new Label(GetDisplayName(entry));
            label.AddToClassList("prefab-browser__item-label");
            item.Add(label);

            GameObject capturedPrefab = entry.prefab;
            string capturedCategory = categoryName;
            VisualElement capturedItem = item;
            item.RegisterCallback<ClickEvent>(_ => OnItemClicked(capturedItem, capturedPrefab, capturedCategory));

            return item;
        }

        private static string GetDisplayName(PrefabEntry entry) =>
            string.IsNullOrEmpty(entry.displayName) ? entry.prefab.name : entry.displayName;

        private void SetupThumbnail(VisualElement thumbnail, PrefabEntry entry)
        {
#if UNITY_EDITOR
            Texture2D preview = UnityEditor.AssetPreview.GetAssetPreview(entry.prefab);
            if (preview != null)
            {
                thumbnail.style.backgroundImage = new StyleBackground(preview);
                return;
            }

            if (UnityEditor.AssetPreview.IsLoadingAssetPreview(entry.prefab.GetInstanceID()))
            {
                thumbnail.schedule.Execute(() => SetupThumbnail(thumbnail, entry)).StartingIn(150);
            }
#endif
        }

        private void OnItemClicked(VisualElement item, GameObject prefab, string categoryName)
        {
            if (_selectedItem != null)
                _selectedItem.RemoveFromClassList(ItemSelectedClass);
            item.AddToClassList(ItemSelectedClass);
            _selectedItem  = item;
            _armedPrefab   = prefab;
            _armedCategory = categoryName;
            _rearmPending  = false;

            StartPlacement(prefab, categoryName);
        }

        private void StartPlacement(GameObject prefab, string categoryName)
        {
            if (_placementPreview == null)
            {
                Debug.LogWarning("[PrefabBrowserView] PlacementPreviewView not assigned.");
                return;
            }

            SceneObject domainObj = new SceneObject
            {
                Id          = System.Guid.NewGuid().ToString(),
                DisplayName = GetDisplayNameForPrefab(prefab, categoryName),
                Category    = categoryName,
                IsImported  = false,
                Transform   = new TransformModel
                {
                    Position = Vector3.zero,
                    Rotation = Quaternion.identity,
                    Scale    = Vector3.one
                }
            };
            ScenePrefabRegistry.Register(domainObj.Id, prefab);
            _placementPreview.BeginPlacement(prefab, domainObj);
        }

        private void Update()
        {
            if (_selectedItem == null || _vm == null) return;
            if (!Input.GetMouseButtonDown(1)) return;
            if (!_vm.IsPlacementMode.Value) return;

            _armedPrefab   = null;
            _armedCategory = null;
            _rearmPending  = false;
            _vm.IsPlacementMode.Value = false;
            _selectedItem.RemoveFromClassList(ItemSelectedClass);
            _selectedItem = null;
        }

        private string GetDisplayNameForPrefab(GameObject prefab, string categoryName)
        {
            BrowserCategory cat = _categories.Find(c => c != null && c.name == categoryName);
            if (cat == null || cat.entries == null) return prefab.name;
            PrefabEntry e = cat.entries.Find(x => x != null && x.prefab == prefab);
            return e != null ? GetDisplayName(e) : prefab.name;
        }
    }
}
