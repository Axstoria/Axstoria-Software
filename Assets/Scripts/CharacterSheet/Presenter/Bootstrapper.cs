using CharacterSheet.App.UseCase;
using CharacterSheet.Domain;
using CharacterSheet.Presenter.ViewModel;
using UnityEngine;
using Loxodon.Framework;
using Loxodon.Framework.Binding;
using Loxodon.Framework.Contexts;

namespace CharacterSheet.Presenter
{
    public class Bootstrapper : MonoBehaviour
    {
        private void Awake()
        {
            // ── Loxodon ───────────────────────────────────────────────────────
            var context = Context.GetApplicationContext();
            var container = context.GetContainer();
            
            BindingServiceBundle bindingBundle = new BindingServiceBundle(container);
            bindingBundle.Start();
            
            // ── Use cases ─────────────────────────────────────────────────────
            container.Register<UpdatePointGaugeWidgetUseCase>(new UpdatePointGaugeWidgetUseCase());
            container.Register<UpdateWidgetAppearanceUseCase>(new UpdateWidgetAppearanceUseCase());
            container.Register<UpdateWidgetLayoutUseCase>(new UpdateWidgetLayoutUseCase());

            var addStat = new AddStatUseCase();
            var removeStat = new RemoveStatUseCase();
            var addWidget = new AddWidgetUseCase();
            var removeWidget = new RemoveWidgetUseCase();
            var bindStatToWidget =  new BindStatToWidgetUseCase();
            var unbindStatToWidget = new UnbindStatUseCase();
            var updateSheet = new UpdateSheetUseCase();
            
            var widgetFactory = new WidgetViewModelFactory(container);
            
            var sheet = new SheetViewModel(new Sheet("0"), 
                widgetFactory,
                addStat,
                removeStat,
                updateSheet,
                addWidget,
                removeWidget,
                bindStatToWidget,
                unbindStatToWidget);
            
            var vm = new CharacterSheetEditorViewModel(sheet);
            
            Context.GetApplicationContext()
                .GetContainer()
                .Register<CharacterSheetEditorViewModel>(vm);
        }
        
    }
}
