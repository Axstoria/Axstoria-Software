using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using CharacterSheet.Presenter.ViewModel;
using Loxodon.Framework.Contexts;
using TMPro;
using UnityEngine;

namespace CharacterSheet.Presenter.View
{
    public class StatsBlockView : MonoBehaviour
    {
        [SerializeField] private Transform container;
        [SerializeField] private GameObject rowTemplate;
        [SerializeField] private GameObject emptyLabel;

        private CharacterSheetEditorViewModel _vm;
        private WidgetViewModel _widget;
        private readonly List<GameObject> _rows = new List<GameObject>();

        private void Start()
        {
            _vm = Context.GetApplicationContext().GetContainer().Resolve<CharacterSheetEditorViewModel>();
            if (_vm == null) return;

            _vm.PropertyChanged += OnEditorPropertyChanged;
            Bind(_vm.SelectedWidget);
        }

        private void OnEditorPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == nameof(CharacterSheetEditorViewModel.SelectedWidget))
                Bind(_vm.SelectedWidget);
        }

        private void Bind(WidgetViewModel widget)
        {
            if (_widget != null) _widget.BoundStats.CollectionChanged -= OnStatsChanged;
            _widget = widget;
            if (_widget != null) _widget.BoundStats.CollectionChanged += OnStatsChanged;
            Rebuild();
        }

        private void OnStatsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Rebuild();
        }

        private void Rebuild()
        {
            foreach (var row in _rows) Destroy(row);
            _rows.Clear();

            if (_widget == null) return;

            foreach (var stat in _widget.BoundStats) {
                var row = Instantiate(rowTemplate, container);
                var label = row.transform.Find("StatName");
                if (label != null) label.GetComponent<TMP_Text>().text = stat.DisplayName;
                row.SetActive(true);
                _rows.Add(row);
            }

            if (emptyLabel != null) emptyLabel.SetActive(_widget.BoundStats.Count == 0);
        }

        private void OnDestroy()
        {
            if (_vm != null) _vm.PropertyChanged -= OnEditorPropertyChanged;
            if (_widget != null) _widget.BoundStats.CollectionChanged -= OnStatsChanged;
        }
    }
}
