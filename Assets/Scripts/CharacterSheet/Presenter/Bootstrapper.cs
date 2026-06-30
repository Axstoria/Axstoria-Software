using AssetImporter.AssetImporter.App;
using AssetImporter.AssetImporter.App.UseCase;
using AssetImporter.AssetImporter.Infrastructure;
using CharacterSheet.App.UseCase;
using CharacterSheet.Domain;
using CharacterSheet.Infrastructure;
using CharacterSheet.Presenter.ViewModel;
using UnityEngine;
using Loxodon.Framework;
using Loxodon.Framework.Binding;
using Loxodon.Framework.Binding.Binders;
using Loxodon.Framework.Contexts;
using Shared.App.Port;
using Shared.Domain;
using Shared.Infrastructure;
using Shared.Infrastructure.Shared.Infrastructure;
using Unity.VisualScripting;

namespace CharacterSheet.Presenter
{
    public class Bootstrapper : MonoBehaviour
    {
        private void Awake()
        {
            // ── Loxodon ───────────────────────────────────────────────────────
            var context = Context.GetApplicationContext();
            var container = context.GetContainer();
            
            if (container.Resolve<IBinder>() == null) {
                BindingServiceBundle bindingBundle = new BindingServiceBundle(container);
                bindingBundle.Start();
            }

            IFileDialogService dialog     = new FileDialogService();
            IStatDefinitionRepository statRepo = new StatDefinitionRepository();
            ISaveRepository jsonRepo = new JsonFileSaveRepository("CharacterSheet");
            var imageService = new ImageService(dialog);
            
            container.Register<IImageLoaderService>(imageService);
            container.Register<IImageImportService>(imageService);
            container.Register<IStatDefinitionRepository>(statRepo);
            container.Register<ISaveRepository>(jsonRepo);
            
            // ── Use cases ─────────────────────────────────────────────────────
            container.Register<UpdatePointGaugeWidgetUseCase>(new UpdatePointGaugeWidgetUseCase());
            container.Register<UpdateTextWidgetUseCase>(new UpdateTextWidgetUseCase());
            container.Register<UpdateWidgetLayoutUseCase>(new UpdateWidgetLayoutUseCase());
            container.Register<UpdateWidgetTitleUseCase>(new UpdateWidgetTitleUseCase());
            container.Register<GetStatUseCase>(new GetStatUseCase(statRepo));
            
            container.Register<UpdateAppearanceUseCase>(new UpdateAppearanceUseCase());
            container.Register<UpdateBackgroundUseCase>(new UpdateBackgroundUseCase(imageService));
            
            container.Register<AddWidgetUseCase>(new AddWidgetUseCase());
            container.Register<RemoveWidgetUseCase>(new RemoveWidgetUseCase());
            container.Register<BindStatToWidgetUseCase>(new BindStatToWidgetUseCase(statRepo));
            container.Register<UnbindStatUseCase>(new UnbindStatUseCase());
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
