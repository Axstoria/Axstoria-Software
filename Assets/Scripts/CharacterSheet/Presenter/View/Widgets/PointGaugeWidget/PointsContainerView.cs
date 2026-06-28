using CharacterSheet.Presenter.ViewModel;
using CharacterSheet.Presenter.ViewModel.Widgets;
using Loxodon.Framework.Binding;
using UnityEngine;

namespace CharacterSheet.Presenter.View.Widgets.PointGaugeWidget
{
    public class PointsContainerView : IStatContainerView
    {
        [SerializeField] private GameObject iconPrefab;

        protected override void Start()
        {
            base.Start();

            _vm = this.BindingContext().DataContext as PointGaugeViewModel.PointItemViewModel;
            if (_vm == null) return;
            
        }
    }
}