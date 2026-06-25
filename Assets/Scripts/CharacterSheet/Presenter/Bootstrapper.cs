using CharacterSheet.App.UseCase;
using CharacterSheet.Domain;
using CharacterSheet.Infrastructure;
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

            IStatDefinitionRepository statRepo = new StatDefinitionRepository();
            container.Register<IStatDefinitionRepository>(statRepo);
            
            // ── Use cases ─────────────────────────────────────────────────────
            container.Register<UpdatePointGaugeWidgetUseCase>(new UpdatePointGaugeWidgetUseCase());
            container.Register<UpdateWidgetAppearanceUseCase>(new UpdateWidgetAppearanceUseCase());
            container.Register<UpdateWidgetLayoutUseCase>(new UpdateWidgetLayoutUseCase());
            container.Register<UpdateWidgetTitleUseCase>(new UpdateWidgetTitleUseCase());
            container.Register<GetStatUseCase>(new GetStatUseCase(statRepo));

            var addStat = new AddStatUseCase(statRepo);
            var removeStat = new RemoveStatUseCase();
            var addWidget = new AddWidgetUseCase();
            var removeWidget = new RemoveWidgetUseCase();
            var bindStatToWidget =  new BindStatToWidgetUseCase(statRepo);
            var unbindStatToWidget = new UnbindStatUseCase();
            var updateSheet = new UpdateSheetUseCase();
            
            var widgetFactory = new WidgetViewModelFactory(container);
            
            var sheet = new SheetViewModel(new Sheet(), 
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
