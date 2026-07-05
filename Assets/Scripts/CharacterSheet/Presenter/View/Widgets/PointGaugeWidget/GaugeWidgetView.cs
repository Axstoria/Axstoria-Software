using CharacterSheet.Presenter.ViewModel;
using CharacterSheet.Presenter.ViewModel.Widgets;
using Loxodon.Framework.Binding;
using UnityEngine;

namespace CharacterSheet.Presenter.View.Widgets.PointGaugeWidget
{
    public class GaugeWidgetView : WidgetView
    {
        [SerializeField] private GameObject pointPrefab;

        private PointGaugeViewModel _vm;

        protected override void Start()
        {
            base.Start();

            _vm = this.BindingContext().DataContext as PointGaugeViewModel;
            if (_vm == null) return;

            var bindingSet = this.CreateBindingSet<WidgetView, WidgetViewModel>();

            bindingSet.Build();
        }
    }
}