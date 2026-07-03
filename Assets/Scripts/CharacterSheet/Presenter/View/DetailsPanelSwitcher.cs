using System.ComponentModel;
using CharacterSheet.Presenter.ViewModel;
using CharacterSheet.Presenter.ViewModel.Widgets;
using Loxodon.Framework.Contexts;
using UnityEngine;

namespace CharacterSheet.Presenter.View
{
    public class DetailsPanelSwitcher : MonoBehaviour
    {
        [SerializeField] private GameObject[] sheetBlocks;
        [SerializeField] private GameObject[] widgetBlocks;
        [SerializeField] private GameObject textBlock;
        [SerializeField] private GameObject counterBlock;
        [SerializeField] private GameObject pointGaugeBlock;
        [SerializeField] private GameObject barBlock;

        private CharacterSheetEditorViewModel _vm;

        private void Start()
        {
            _vm = Context.GetApplicationContext().GetContainer().Resolve<CharacterSheetEditorViewModel>();
            if (_vm == null) return;

            _vm.PropertyChanged += OnPropertyChanged;
            Apply();
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == nameof(CharacterSheetEditorViewModel.SelectedWidget))
                Apply();
        }

        private void Apply()
        {
            var widget = _vm.SelectedWidget;
            bool hasSelection = widget != null;
            foreach (var block in sheetBlocks) block.SetActive(!hasSelection);
            foreach (var block in widgetBlocks) block.SetActive(hasSelection);
            if (textBlock != null) textBlock.SetActive(widget is TextWidgetViewModel);
            if (counterBlock != null) counterBlock.SetActive(widget is CounterWidgetViewModel);
            if (pointGaugeBlock != null) pointGaugeBlock.SetActive(widget is PointGaugeViewModel);
            if (barBlock != null) barBlock.SetActive(widget is BarWidgetViewModel);
        }

        private void OnDestroy()
        {
            if (_vm != null) _vm.PropertyChanged -= OnPropertyChanged;
        }
    }
}
