using CharacterSheet.Presenter.ViewModel;
using CharacterSheet.Presenter.ViewModel.Widgets;
using Loxodon.Framework.Binding;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

namespace CharacterSheet.Presenter.View.Widgets.CounterWidget
{
    public class CounterItemView : IStatContainerView
    {
        [SerializeField] protected TextMeshProUGUI textMesh;
        
        protected override void Start()
        {
            base.Start();

            _vm = this.BindingContext().DataContext as CounterWidgetViewModel.CounterItemViewModel;
            if (_vm == null) return;
            
            var bindingSet = this.CreateBindingSet<CounterItemView, CounterWidgetViewModel.CounterItemViewModel>();
            bindingSet.Bind(textMesh).For(v => v.text).To(vm => vm.FormattedText);

            bindingSet.Build();
        }
    }
}