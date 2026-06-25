using System;
using CharacterSheet.Domain.Widgets;
using CharacterSheet.Presenter.ViewModel;
using Loxodon.Framework.Binding;
using Loxodon.Framework.Views;
using UnityEngine;

namespace CharacterSheet.Presenter.View
{
    public class DynamicConfigPanelContainer : UIView
    {
        [SerializeField] private Transform containerParent;
        /*[SerializeField] private PointGaugeConfigPanelView _pointGaugeConfigPrefab;
        [SerializeField] private TextWidgetConfigPanelView _textWidgetConfigPrefab;*/
        
        private ConfigurationPanelViewBase _currentActiveView;
        protected override void Start()
        {
            base.Start();
            
            var mainVM = this.BindingContext().DataContext as CharacterSheetEditorViewModel;
            if (mainVM == null) return;
            
            var bindingSet = this.CreateBindingSet<DynamicConfigPanelContainer, CharacterSheetEditorViewModel>();

            bindingSet.Bind(this).For(v => v.OnSelectedWidgetChanged).To(vm => vm.SelectedWidget);

            bindingSet.Bind();
        }

        private Action<WidgetViewModel> OnSelectedWidgetChanged => (selectedWidget) =>
        {
            if (selectedWidget == null) return;

            ConfigurationPanelViewBase prefabToSpawn = selectedWidget switch
            {
                _ => null
            };

            if (prefabToSpawn != null)
            {
                _currentActiveView = Instantiate(prefabToSpawn, containerParent);

                _currentActiveView.SetDataContext(selectedWidget);
            }
        };
    }
}