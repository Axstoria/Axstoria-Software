using CharacterSheet.App.UseCase;
using CharacterSheet.Domain;
using CharacterSheet.Infrastructure;
using CharacterSheet.Presenter.ViewModel;
using UnityEngine;
using Loxodon.Framework;
using Loxodon.Framework.Binding;
using Loxodon.Framework.Contexts;
using Shared.Domain;
using Shared.Infrastructure;

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
            ISaveRepository jsonRepo = new JsonFileSaveRepository("CharacterSheet");
            container.Register<ISaveRepository>(jsonRepo);
            
            // ── Use cases ─────────────────────────────────────────────────────
            container.Register<UpdatePointGaugeWidgetUseCase>(new UpdatePointGaugeWidgetUseCase());
            container.Register<UpdateWidgetAppearanceUseCase>(new UpdateWidgetAppearanceUseCase());
            container.Register<UpdateWidgetLayoutUseCase>(new UpdateWidgetLayoutUseCase());
            container.Register<UpdateWidgetTitleUseCase>(new UpdateWidgetTitleUseCase());
            container.Register<GetStatUseCase>(new GetStatUseCase(statRepo));
            
            container.Register<AddStatUseCase>(new AddStatUseCase(statRepo));
            container.Register<RemoveStatUseCase>(new RemoveStatUseCase());
            container.Register<AddWidgetUseCase>(new AddWidgetUseCase());
            container.Register<RemoveWidgetUseCase>(new RemoveWidgetUseCase());
            container.Register<BindStatToWidgetUseCase>(new BindStatToWidgetUseCase(statRepo));
            container.Register<UnbindStatUseCase>(new UnbindStatUseCase());
            container.Register<UpdateSheetUseCase>(new UpdateSheetUseCase());
            container.Register<SaveUseCases>(new SaveUseCases(jsonRepo));
            container.Register<LoadUseCases>(new LoadUseCases(jsonRepo));
            
            var widgetFactory = new WidgetViewModelFactory(container);
            container.Register<WidgetViewModelFactory>(widgetFactory);
            var sheetFactory = new SheetViewModelFactory(container);
            
            var vm = new CharacterSheetEditorViewModel(sheetFactory, 
                container.Resolve<LoadUseCases>(),
                container.Resolve<SaveUseCases>());
            
            Context.GetApplicationContext()
                .GetContainer()
                .Register<CharacterSheetEditorViewModel>(vm);
        }
        
    }
}
