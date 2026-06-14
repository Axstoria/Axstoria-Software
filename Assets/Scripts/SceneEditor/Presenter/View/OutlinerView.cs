using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Loxodon.Framework.Contexts;
using MapEditor.Presenter.View;
using MapEditor.Presenter.ViewModels;
using SceneEditor.Presenter.ViewModels;
using UnityEngine;
using UnityEngine.UIElements;

namespace SceneEditor.Presenter.View
{
    public class OutlinerView : MonoBehaviour
    {
        private const string RowClass         = "outliner__row";
        private const string RowSelectedClass = "outliner__row--selected";
        private const string NameClass        = "outliner__name";

        private MapEditorViewModel     _vm;
        private TransformGizmoView     _gizmo;
        private SceneObjectSpawnerView _spawner;

        private TextField     _search;
        private VisualElement _list;
        private string        _query = "";

        private readonly Dictionary<string, VisualElement> _rows = new();
        private VisualElement _selectedRow;

        private NotifyCollectionChangedEventHandler _onObjectsChanged;

        public void Init(VisualElement panel)
        {
            _vm = Context.GetApplicationContext().GetContainer().Resolve<MapEditorViewModel>();
            if (_vm == null)
            {
                Debug.LogError("[OutlinerView] MapEditorViewModel not registered.");
                enabled = false;
                return;
            }

            _gizmo   = FindFirstObjectByType<TransformGizmoView>();
            _spawner = FindFirstObjectByType<SceneObjectSpawnerView>();

            _search = panel.Q<TextField>("outliner-search");
            _list   = panel.Q<VisualElement>("outliner-list");

            if (_search != null)
                _search.RegisterValueChangedCallback(e => Filter(e.newValue));

            foreach (ObjectViewModel obj in _vm.Map.Objects)
                AddEntry(obj);

            _onObjectsChanged = OnObjectsChanged;
            _vm.Map.Objects.CollectionChanged += _onObjectsChanged;
        }

        private void OnDestroy()
        {
            if (_vm != null && _onObjectsChanged != null)
                _vm.Map.Objects.CollectionChanged -= _onObjectsChanged;
        }

        private void OnObjectsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (ObjectViewModel obj in e.NewItems)
                    AddEntry(obj);
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (ObjectViewModel obj in e.OldItems)
                    RemoveEntry(obj.Model.Id);
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                _list.Clear();
                _rows.Clear();
                _selectedRow = null;
            }
        }

        private void AddEntry(ObjectViewModel obj)
        {
            var row = new VisualElement { name = obj.Model.Id };
            row.AddToClassList(RowClass);

            var label = new Label(DisplayNameOf(obj));
            label.AddToClassList(NameClass);
            obj.DisplayName.ValueChanged += (_, __) =>
            {
                label.text = DisplayNameOf(obj);
                ApplyFilter(row, label.text);
            };

            var delete = new Button { text = "×" };
            delete.AddToClassList("outliner__delete");
            delete.tooltip = "Delete";
            delete.RegisterCallback<ClickEvent>(evt =>
            {
                evt.StopPropagation();
                _vm.DeleteObject.Execute(obj.Model);
            });

            row.RegisterCallback<ClickEvent>(_ => Select(obj, row));

            row.Add(label);
            row.Add(delete);
            _list.Add(row);
            _rows[obj.Model.Id] = row;

            ApplyFilter(row, label.text);
        }

        private void RemoveEntry(string id)
        {
            if (!_rows.TryGetValue(id, out VisualElement row)) return;
            if (row == _selectedRow) _selectedRow = null;
            row.RemoveFromHierarchy();
            _rows.Remove(id);
        }

        private void Select(ObjectViewModel obj, VisualElement row)
        {
            _selectedRow?.RemoveFromClassList(RowSelectedClass);
            row.AddToClassList(RowSelectedClass);
            _selectedRow = row;

            if (_gizmo != null && _spawner != null &&
                _spawner.TryGetGameObject(obj.Model.Id, out GameObject go))
                _gizmo.Select(go, obj.Model);
        }

        private void Filter(string query)
        {
            _query = query ?? "";
            foreach (KeyValuePair<string, VisualElement> kvp in _rows)
            {
                Label label = kvp.Value.Q<Label>(className: NameClass);
                ApplyFilter(kvp.Value, label?.text ?? "");
            }
        }

        private void ApplyFilter(VisualElement row, string displayName)
        {
            bool visible = string.IsNullOrEmpty(_query)
                        || displayName.IndexOf(_query, StringComparison.OrdinalIgnoreCase) >= 0;
            row.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private static string DisplayNameOf(ObjectViewModel obj)
            => string.IsNullOrEmpty(obj.DisplayName.Value) ? "(unnamed)" : obj.DisplayName.Value;
    }
}
