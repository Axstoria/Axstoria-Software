using System.ComponentModel;
using CharacterSheet.Domain;
using CharacterSheet.Presenter.ViewModel;
using Loxodon.Framework.Binding;
using Loxodon.Framework.Contexts;
using Loxodon.Framework.Views;
using UnityEngine;
using UnityEngine.UI;

namespace CharacterSheet.Presenter.View
{
    public class CharacterSheetEditorView : UIView
    {
        [SerializeField] private Transform widgetContainer;
        [SerializeField] private Button addStatButton;
        [SerializeField] private Button addWidgetButton;
        [SerializeField] private GameObject sheetPrefab;

        private CharacterSheetEditorViewModel _vm;
        protected override void Awake()
        {
            base.Awake();
            
            _vm = Context.GetApplicationContext().GetContainer().Resolve<CharacterSheetEditorViewModel>();
            this.SetDataContext(_vm);
            
            var sheetViewModel = _vm.CurrentSheet;
            if (sheetViewModel != null)
            {
                var go = Instantiate(sheetPrefab, widgetContainer);
                var view = go.GetComponent<SheetView>();
                view.SetDataContext(sheetViewModel);
                view.Initialize(sheetViewModel);
            }
            
            CreateBindings();
        }
        
        private void CreateBindings()
        {
           var bindingSet = this.CreateBindingSet<CharacterSheetEditorView, CharacterSheetEditorViewModel>();

           bindingSet.Bind(addWidgetButton)
               .For(v => v.onClick)
               .To(x => x.AddWidgetCommand)
               .CommandParameter(WidgetType.PointGauge);
           
           /*bindingSet.Bind(addStatButton)
               .For(v => v.onClick)
               .To(x => x.AddStatCommand);*/
           
           bindingSet.Build();
        }

        /*private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CharacterSheetEditorViewModel.SelectedWidget))
                OnSelectedWidgetChanged(OnSelectedWidgetChanged(_vm.SelectedWidget));
        }

        private void OnSelectedWidgetChanged(WidgetViewModel widget)
        {
            
        }*/
    }
}