using System.Collections.Generic;
using System.ComponentModel;
using CharacterSheet.Presenter.ViewModel;
using Loxodon.Framework.Contexts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CharacterSheet.Presenter.View
{
    public class StatsBlockView : MonoBehaviour
    {
        [SerializeField] private Transform container;
        [SerializeField] private GameObject rowTemplate;
        [SerializeField] private GameObject emptyLabel;

        private CharacterSheetEditorViewModel _vm;
        private WidgetViewModel _widget;
        private readonly Dictionary<string, List<(string Id, string Name)>> _statsByWidget = new();
        private readonly List<GameObject> _rows = new();

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
            _widget = widget;
            Rebuild();
        }

        public bool TryAddStat(string statId, string displayName)
        {
            if (_widget == null || string.IsNullOrEmpty(statId)) return false;
            var stats = GetStats(_widget.Id);
            if (stats.Exists(s => s.Id == statId)) return false;
            _widget.AddStatCommand.Execute(statId);
            stats.Add((statId, displayName));
            Rebuild();
            return true;
        }

        private void RemoveStat(string statId)
        {
            if (_widget == null) return;
            _widget.RemoveStatCommand.Execute(statId);
            GetStats(_widget.Id).RemoveAll(s => s.Id == statId);
            Rebuild();
        }

        private List<(string Id, string Name)> GetStats(string widgetId)
        {
            if (!_statsByWidget.TryGetValue(widgetId, out var stats)) {
                stats = new List<(string, string)>();
                _statsByWidget[widgetId] = stats;
            }
            return stats;
        }

        private void Rebuild()
        {
            foreach (var row in _rows) Destroy(row);
            _rows.Clear();

            if (_widget == null) return;

            var stats = GetStats(_widget.Id);
            foreach (var stat in stats) {
                var row = Instantiate(rowTemplate, container);
                var label = row.transform.Find("StatName");
                if (label != null) label.GetComponent<TMP_Text>().text = stat.Name;
                var unbind = row.transform.Find("Btn_Unbind");
                if (unbind != null) {
                    string statId = stat.Id;
                    unbind.GetComponent<Button>().onClick.AddListener(() => RemoveStat(statId));
                }
                row.SetActive(true);
                _rows.Add(row);
            }

            if (emptyLabel != null) emptyLabel.SetActive(stats.Count == 0);
        }

        private void OnDestroy()
        {
            if (_vm != null) _vm.PropertyChanged -= OnEditorPropertyChanged;
        }
    }
}
