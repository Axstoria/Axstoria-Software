using System.Collections.Specialized;
using CharacterSheet.Presenter.ViewModel;
using CharacterSheet.Presenter.ViewModel.Widgets;
using Loxodon.Framework.Binding;
using TMPro;
using UnityEngine;

namespace CharacterSheet.Presenter.View.Widgets.TextWidget
{
    public class TextWidgetView : WidgetView
    {
        [SerializeField] private TMP_InputField inputField;

        private TextWidgetViewModel _vm;

        public string DisplayedText
        {
            get => inputField.text;
            set
            {
                if (inputField != null && !inputField.isFocused) {
                    inputField.text = value;
                }
            }
        }

        protected override void Start()
        {
            base.Start();

            _vm = this.BindingContext().DataContext as TextWidgetViewModel;
            if (_vm == null) return;

            var bindingSet = this.CreateBindingSet<TextWidgetView, TextWidgetViewModel>();

            inputField.text = _vm.DisplayedText;

            inputField.onSelect.AddListener(OnStartEditing);

            inputField.onDeselect.AddListener(OnEndEditing);

            bindingSet.Bind(this)
                .For(v => v.DisplayedText)
                .To(vm => vm.DisplayedText);

            bindingSet.Build();
        }

        private void OnStartEditing(string currentText)
        {
            inputField.text = _vm.TextTemplate;
        }

        private void OnEndEditing(string newTemplateValue)
        {
            _vm.UpdateContentCommand.Execute(newTemplateValue);

            inputField.text = _vm.DisplayedText;
        }

        protected override void OnStatAdded(WidgetItemViewModel newBoundStat)
        {
        }

        protected override void OnStatRemoved(WidgetItemViewModel boundStat)
        {
        }
    }
}