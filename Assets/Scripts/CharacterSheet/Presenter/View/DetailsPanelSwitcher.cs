using System.ComponentModel;
using CharacterSheet.Presenter.ViewModel;
using Loxodon.Framework.Contexts;
using UnityEngine;

namespace CharacterSheet.Presenter.View
{
    public class DetailsPanelSwitcher : MonoBehaviour
    {
        [SerializeField] private GameObject[] sheetBlocks;
        [SerializeField] private GameObject[] widgetBlocks;

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
            bool hasSelection = _vm.SelectedWidget != null;
            foreach (var block in sheetBlocks) block.SetActive(!hasSelection);
            foreach (var block in widgetBlocks) block.SetActive(hasSelection);
        }

        private void OnDestroy()
        {
            if (_vm != null) _vm.PropertyChanged -= OnPropertyChanged;
        }
    }
}
