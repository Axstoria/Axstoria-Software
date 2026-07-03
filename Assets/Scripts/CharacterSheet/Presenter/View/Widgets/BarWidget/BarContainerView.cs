using System.Collections.Generic;
using System.Collections.Specialized;
using CharacterSheet.Presenter.ViewModel.Widgets;
using Loxodon.Framework.Binding;
using UnityEngine;
using UnityEngine.UI;

namespace CharacterSheet.Presenter.View.Widgets.BarWidget
{
    public class BarContainerView : IStatContainerView
    {
        [SerializeField] private Image mask;
        private BarWidgetViewModel.ProgressionBarViewModel _itemViewModel;

        protected override void Start()
        {
            base.Start();

            _itemViewModel = this.BindingContext().DataContext as BarWidgetViewModel.ProgressionBarViewModel;
            _vm = _itemViewModel;
            if (_itemViewModel == null) return;
            
            var bindingSet = this.CreateBindingSet<BarContainerView, BarWidgetViewModel.ProgressionBarViewModel>();

            bindingSet.Bind(mask)
                .For(v => v.fillAmount)
                .To(vm => vm.Fill);
            
            bindingSet.Build();
        }
        
    }
}