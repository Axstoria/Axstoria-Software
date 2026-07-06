using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Loxodon.Framework.Contexts;
using MapEditor.Presenter.ViewModels;
using SceneEditor.Domain;
using SceneEditor.Presenter.ViewModels;
using UnityEngine;
using UnityEngine.UIElements;

namespace SceneEditor.Presenter.View
{
    public class MetadataPopupController : MonoBehaviour
    {
        // Popup panels
        private VisualElement _panelNotes;
        private VisualElement _panelTags;
        private VisualElement _panelSheets;

        // Notes panel
        private DropdownField _objectDropdownNotes;
        private TextField _noteField;

        // Tags panel
        private DropdownField _objectDropdownTags;
        private DropdownField _tagDropdown;


        private VisualElement _overlay;
        private DropdownField _categoryDropdown;
        private Button _confirmButton;
        private Button _cancelButton;
        private TemplateContainer _popupElement;
        private VisualElement _notesList;
        private Label _labelNoNotes;

        private List<ObjectViewModel> _placedObjects = new();
        private List<TagViewModel> _tags = new();
        private NotifyCollectionChangedEventHandler _onObjectsChanged;
        private NotifyCollectionChangedEventHandler _onTagsChanged;
        private MapEditorViewModel _vm;

        private static readonly List<string> Categories = new() { "Notes", "Tags", "Sheets" };

        public void Init(VisualElement root)
        {
            VisualTreeAsset asset = Resources.Load<VisualTreeAsset>("MetadataPopup/Popup");
            _popupElement = asset.Instantiate();
            root.Add(_popupElement);

            _overlay = _popupElement.Q<VisualElement>("overlay");
            _categoryDropdown = _popupElement.Q<DropdownField>("category-dropdown");
            _confirmButton = _popupElement.Q<Button>("confirm-button");
            _cancelButton = _popupElement.Q<Button>("cancel-button");

            _panelNotes = _popupElement.Q<VisualElement>("panel-notes");
            _panelTags = _popupElement.Q<VisualElement>("panel-tags");
            _panelSheets = _popupElement.Q<VisualElement>("panel-sheets");

            _objectDropdownNotes = _popupElement.Q<DropdownField>("object-dropdown-notes");
            _noteField = _popupElement.Q<TextField>("note-field");
            _objectDropdownTags = _popupElement.Q<DropdownField>("object-dropdown-tags");
            _tagDropdown = _popupElement.Q<DropdownField>("tag-dropdown");

            _notesList = root.Q<VisualElement>("notes-list");
            _labelNoNotes = root.Q<Label>("label-no-notes");

            _categoryDropdown.choices = Categories;
            _categoryDropdown.RegisterValueChangedCallback(evt => ShowPanel(evt.newValue));

            _confirmButton.clicked += OnConfirm;
            _cancelButton.clicked += Close;

            _overlay.style.display = DisplayStyle.None;
        }

        public void Open()
        {
            _vm = Context.GetApplicationContext().GetContainer().Resolve<MapEditorViewModel>();
            if (_vm == null)
            {
                Debug.Log("[EditionToolbarUIManager] MapEditorViewModel not registered");
                return;
            }

            _onObjectsChanged = OnPlacedObjectsChanged;
            _vm.Map.Objects.CollectionChanged += _onObjectsChanged;

            _onTagsChanged = (_, __) => RefreshTagDropdown();
            _vm.Map.Tags.CollectionChanged += _onTagsChanged;

            RefreshObjectDropdowns();
            RefreshTagDropdown();

            _categoryDropdown.index = 0;
            ShowPanel(Categories[0]);
            _overlay.style.display = DisplayStyle.Flex;
        }

        private void ShowPanel(string category)
        {
            _panelNotes.style.display = category == "Notes" ? DisplayStyle.Flex : DisplayStyle.None;
            _panelTags.style.display = category == "Tags" ? DisplayStyle.Flex : DisplayStyle.None;
            _panelSheets.style.display = category == "Sheets" ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void RefreshObjectDropdowns()
        {
            _placedObjects = new List<ObjectViewModel>(_vm.Map.Objects);
            var names = _placedObjects.ConvertAll(o => ComputeName(o));

            _objectDropdownNotes.choices = names;
            _objectDropdownNotes.index = names.Count > 0 ? 0 : -1;
            _objectDropdownTags.choices = names;
            _objectDropdownTags.index = names.Count > 0 ? 0 : -1;
        }

        private string ComputeName(ObjectViewModel obj)
        {
            string baseName = BaseNameOf(obj);
            int index = 0, total = 0;
            foreach (ObjectViewModel other in _vm.Map.Objects)
            {
                if (BaseNameOf(other) != baseName) continue;
                if (other.Model.Id == obj.Model.Id) index = total;
                total++;
            }
            if (total <= 1) return baseName;
            return index == 0 ? baseName : $"{baseName} ({index + 1})";
        }

        private static string BaseNameOf(ObjectViewModel obj)
            => string.IsNullOrEmpty(obj.DisplayName.Value) ? "(unnamed)" : obj.DisplayName.Value;

        private void RefreshTagDropdown()
        {
            _tags = new List<TagViewModel>(_vm.Map.Tags);
            _tagDropdown.choices = _tags.Select(t => t.Name.Value).ToList();
            _tagDropdown.index = _tags.Count > 0 ? 0 : -1;
        }

        private void OnPlacedObjectsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            int prevNotes = _objectDropdownNotes.index;
            int prevTags = _objectDropdownTags.index;

            RefreshObjectDropdowns();

            _objectDropdownNotes.index = Mathf.Clamp(prevNotes, 0, _placedObjects.Count - 1);
            _objectDropdownTags.index = Mathf.Clamp(prevTags, 0, _placedObjects.Count - 1);
        }

        public void Close()
        {
            if (_vm != null && _onObjectsChanged != null)
            {
                _vm.Map.Objects.CollectionChanged -= _onObjectsChanged;
                _onObjectsChanged = null;
            }
            if (_vm != null && _onTagsChanged != null)
            {
                _vm.Map.Tags.CollectionChanged -= _onTagsChanged;
                _onTagsChanged = null;
            }
            _noteField.value = "";
            _overlay.style.display = DisplayStyle.None;
        }

        public void OnConfirm()
        {
            var vm = Context.GetApplicationContext().GetContainer().Resolve<MapEditorViewModel>();
            if (vm == null)
                return;

            switch (_categoryDropdown.value)
            {
                case "Notes":
                    if (_objectDropdownNotes.index < 0)
                        return;
                    SceneObject noteTarget = _placedObjects[_objectDropdownNotes.index].Model;
                    if (noteTarget == null)
                        return;
                    vm.SetObjectMetadata.Execute(noteTarget, "note", new NoteValue { Text = _noteField.value });
                    break;
                case "Tags":
                    if (_objectDropdownTags.index < 0 || _tagDropdown.index < 0)
                        return;
                    SceneObject tagTarget = _placedObjects[_objectDropdownTags.index].Model;
                    TagValue selectedTag = _tags[_tagDropdown.index].Model;
                    if (tagTarget == null || selectedTag == null)
                        return;
                    bool alreadyAssigned = tagTarget.Metadata?.Any(e =>
                        e.EntryValue is TagValue tv && tv.Id == selectedTag.Id) == true;
                    if (!alreadyAssigned)
                        vm.SetObjectMetadata.Execute(tagTarget, "tag", new TagValue { Id = selectedTag.Id });
                    break;
                case "Sheets":
                    // TODO: to be implemented when sheets are done
                    break;
            }
            Close();
        }
    }
}
