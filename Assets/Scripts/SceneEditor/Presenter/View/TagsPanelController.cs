using System.Collections.Specialized;
using Loxodon.Framework.Contexts;
using MapEditor.Presenter.ViewModels;
using UnityEngine;
using UnityEngine.UIElements;

namespace SceneEditor.Presenter.View
{
    public class TagsPanelController : MonoBehaviour
    {
        private TextField _newNameField;
        private VisualElement _newSwatch;
        private Button _addButton;
        private VisualElement _tagList;

        private ColorPickerPopupController _colorPicker;
        private MapEditorViewModel _vm;
        private string _newHexColor = "#FFFFFF";
        private NotifyCollectionChangedEventHandler _onTagsChanged;

        public void Init(VisualElement root, ColorPickerPopupController colorPicker)
        {
            _colorPicker = colorPicker;

            _newNameField = root.Q<TextField>("new-tag-name-field");
            _newSwatch    = root.Q<VisualElement>("new-tag-swatch");
            _addButton    = root.Q<Button>("add-tag-button");
            _tagList      = root.Q<VisualElement>("tag-manager-list");

            ApplySwatchColor(_newSwatch, _newHexColor);
            _newSwatch.RegisterCallback<ClickEvent>(_ =>
                _colorPicker.Open(_newSwatch, _newHexColor, hex =>
                {
                    _newHexColor = hex;
                    ApplySwatchColor(_newSwatch, hex);
                }));

            _addButton.clicked += OnAddTag;

            _vm = Context.GetApplicationContext().GetContainer().Resolve<MapEditorViewModel>();
            if (_vm == null)
            {
                Debug.LogWarning("[TagsPanelController] MapEditorViewModel not registered");
                return;
            }

            RebuildList();
            _onTagsChanged = (_, __) => RebuildList();
            _vm.Map.Tags.CollectionChanged += _onTagsChanged;
        }

        private void OnDestroy()
        {
            if (_vm != null && _onTagsChanged != null)
                _vm.Map.Tags.CollectionChanged -= _onTagsChanged;
        }

        private void OnAddTag()
        {
            string name = _newNameField.value;
            if (string.IsNullOrWhiteSpace(name)) return;

            _vm.CreateTag.Execute(name, _newHexColor);

            _newNameField.value = "";
            _newHexColor = "#FFFFFF";
            ApplySwatchColor(_newSwatch, _newHexColor);
        }

        private void RebuildList()
        {
            _tagList.Clear();
            foreach (TagViewModel tag in _vm.Map.Tags)
                AddRow(tag);
        }

        private void AddRow(TagViewModel tag)
        {
            var row = new VisualElement();
            row.AddToClassList("tag-manager-row");

            var nameField = new TextField { value = tag.Name.Value };
            nameField.AddToClassList("tag-manager-name-field");

            var swatch = new VisualElement();
            swatch.AddToClassList("tag-manager-swatch");
            ApplySwatchColor(swatch, tag.HexColor.Value);
            swatch.RegisterCallback<ClickEvent>(_ =>
                _colorPicker.Open(swatch, tag.HexColor.Value, hex =>
                {
                    ApplySwatchColor(swatch, hex);
                    _vm.RenameTag.Execute(tag.Model, nameField.value, hex);
                }));

            var saveButton = new Button { text = "Save" };
            saveButton.AddToClassList("tag-manager-save-button");
            saveButton.clicked += () => _vm.RenameTag.Execute(tag.Model, nameField.value, tag.HexColor.Value);

            var deleteButton = new Button { text = "×" };
            deleteButton.AddToClassList("tag-manager-delete-button");
            deleteButton.RegisterCallback<ClickEvent>(evt =>
            {
                evt.StopPropagation();
                _vm.DeleteTag.Execute(tag.Entry);
            });

            row.Add(swatch);
            row.Add(nameField);
            row.Add(saveButton);
            row.Add(deleteButton);
            _tagList.Add(row);
        }

        private static void ApplySwatchColor(VisualElement swatch, string hex)
        {
            if (ColorUtility.TryParseHtmlString(hex, out Color c))
                swatch.style.backgroundColor = c;
        }
    }
}
