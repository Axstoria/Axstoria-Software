using CharacterSheet.Presenter.ViewModel;
using Loxodon.Framework.Binding;
using Loxodon.Framework.Views;
using Unity.VisualScripting;
using UnityEngine;

namespace CharacterSheet.Presenter.View
{
    public class PointsContainerView : IStatContainerView
    {
        [SerializeField] private GameObject iconPrefab;
        
        private StatViewModel _vm;

        protected override void Start()
        {
            base.Start();
            
            _vm = this.BindingContext().DataContext as StatViewModel;
            if(_vm == null) return;
            
            for (int i = 0; i < _vm.MaxValue; i++)
            {
                GameObject go = Instantiate(iconPrefab, transform);
            }
        }
    }
}