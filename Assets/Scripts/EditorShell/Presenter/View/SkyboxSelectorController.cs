using System;
using System.Collections.Generic;
using Loxodon.Framework.Contexts;
using MapEditor.Presenter.ViewModels;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

namespace EditorShell.Presenter.View
{
    [Serializable]
    public class SkyboxOption
    {
        public string displayName;
        public Material material;
        public Texture2D preview;
    }

    public class SkyboxSelectorController : MonoBehaviour
    {
        private const string ItemSelectedClass = "skybox-item--selected";

        [SerializeField] private List<SkyboxOption> _skyboxes = new();
        [SerializeField] private int _defaultIndex = 0;

        private VisualElement _grid;
        private readonly Dictionary<int, VisualElement> _items = new();
        private VisualElement _selectedItem;
        private int _firstValidIndex = -1;
        private MapEditorViewModel _vm;

        public void Init(VisualElement root)
        {
            _vm = Context.GetApplicationContext().GetContainer().Resolve<MapEditorViewModel>();

            _grid = root.Q<VisualElement>("skybox-grid");
            if (_grid == null)
            {
                Debug.LogWarning("[SkyboxSelectorController] 'skybox-grid' not found in root.");
                return;
            }

            BuildGrid();

            if (_firstValidIndex < 0) return;

            string savedName = _vm?.Map?.Model?.SkyboxName;
            int index = !string.IsNullOrEmpty(savedName) && TryFindIndexByName(savedName, out int found)
                ? found
                : (_items.ContainsKey(_defaultIndex) ? _defaultIndex : _firstValidIndex);

            SelectSkybox(index);
        }

        // Called after a map import to reselect whichever skybox the loaded map had.
        public void SelectByName(string name)
        {
            if (string.IsNullOrEmpty(name)) return;
            if (TryFindIndexByName(name, out int index))
                SelectSkybox(index);
        }

        private bool TryFindIndexByName(string name, out int index)
        {
            for (int i = 0; i < _skyboxes.Count; i++)
            {
                if (_skyboxes[i]?.material == null) continue;
                if (LabelOf(_skyboxes[i]) == name) { index = i; return true; }
            }
            index = -1;
            return false;
        }

        private static string LabelOf(SkyboxOption option)
            => string.IsNullOrEmpty(option.displayName) ? option.material.name : option.displayName;

        private void BuildGrid()
        {
            _grid.Clear();
            _items.Clear();
            _firstValidIndex = -1;

            for (int i = 0; i < _skyboxes.Count; i++)
            {
                SkyboxOption option = _skyboxes[i];
                if (option == null || option.material == null) continue;

                if (_firstValidIndex < 0) _firstValidIndex = i;

                VisualElement item = BuildItem(option, i);
                _items[i] = item;
                _grid.Add(item);
            }
        }

        private VisualElement BuildItem(SkyboxOption option, int index)
        {
            VisualElement item = new VisualElement();
            item.AddToClassList("skybox-item");

            VisualElement thumbnail = new VisualElement();
            thumbnail.AddToClassList("skybox-item__thumbnail");
            if (option.preview != null)
                thumbnail.style.backgroundImage = new StyleBackground(option.preview);
            item.Add(thumbnail);

            Label label = new Label(LabelOf(option));
            label.AddToClassList("skybox-item__label");
            item.Add(label);

            item.RegisterCallback<ClickEvent>(_ => SelectSkybox(index));

            return item;
        }

        private void SelectSkybox(int index)
        {
            if (!_items.TryGetValue(index, out VisualElement item)) return;
            SkyboxOption option = _skyboxes[index];

            RenderSettings.skybox = option.material;
            DynamicGI.UpdateEnvironment();

            if (_selectedItem != null)
                _selectedItem.RemoveFromClassList(ItemSelectedClass);
            _selectedItem = item;
            _selectedItem.AddToClassList(ItemSelectedClass);

            if (_vm?.Map?.Model != null)
                _vm.Map.Model.SkyboxName = LabelOf(option);
        }
    }
}
