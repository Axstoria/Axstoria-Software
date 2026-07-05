using System.Threading;
using AssetImporter.AssetImporter.App.UseCase;
using CharacterSheet.App.UseCase;
using CharacterSheet.Domain;
using CharacterSheet.Domain.Widgets;
using CharacterSheet.Presenter.ViewModel.Widgets;
using Loxodon.Framework.Services;

namespace CharacterSheet.Presenter.ViewModel
{
    public class WidgetViewModelFactory
    {
        private readonly IServiceContainer _container;

        public WidgetViewModelFactory(IServiceContainer container)
        {
            _container = container;
        }

        // TODO: Create dependencies wrapper.
        public WidgetViewModel Create(SheetWidget widget, string containerId = "") => widget switch
        {
            PointGaugeWidget g => new PointGaugeViewModel(g,
                containerId,
                _container.Resolve<BindStatToWidgetUseCase>(),
                _container.Resolve<UnbindStatUseCase>(),
                _container.Resolve<UpdateAppearanceUseCase>(),
                _container.Resolve<UpdateBackgroundUseCase>(),
                _container.Resolve<IImageLoaderService>(),
                _container.Resolve<UpdateWidgetLayoutUseCase>(),
                _container.Resolve<UpdateWidgetTitleUseCase>(),
                _container.Resolve<GetStatUseCase>(),
                _container.Resolve<UpdatePointGaugeWidgetUseCase>()),
            BarWidget b => new BarWidgetViewModel(b,
                containerId,
                _container.Resolve<BindStatToWidgetUseCase>(),
                _container.Resolve<UnbindStatUseCase>(),
                _container.Resolve<UpdateAppearanceUseCase>(),
                _container.Resolve<UpdateBackgroundUseCase>(),
                _container.Resolve<IImageLoaderService>(),
                _container.Resolve<UpdateWidgetLayoutUseCase>(),
                _container.Resolve<UpdateWidgetTitleUseCase>(),
                _container.Resolve<GetStatUseCase>()),
            TextWidget t => new TextWidgetViewModel(t,
                containerId,
                _container.Resolve<BindStatToWidgetUseCase>(),
                _container.Resolve<UnbindStatUseCase>(),
                _container.Resolve<UpdateAppearanceUseCase>(),
                _container.Resolve<UpdateBackgroundUseCase>(),
                _container.Resolve<IImageLoaderService>(),
                _container.Resolve<UpdateWidgetLayoutUseCase>(),
                _container.Resolve<UpdateWidgetTitleUseCase>(),
                _container.Resolve<GetStatUseCase>(),
                _container.Resolve<UpdateTextWidgetUseCase>()),
            CounterWidget c => new CounterWidgetViewModel(c,
                containerId,
                _container.Resolve<BindStatToWidgetUseCase>(),
                _container.Resolve<UnbindStatUseCase>(),
                _container.Resolve<UpdateAppearanceUseCase>(),
                _container.Resolve<UpdateBackgroundUseCase>(),
                _container.Resolve<IImageLoaderService>(),
                _container.Resolve<UpdateWidgetLayoutUseCase>(),
                _container.Resolve<UpdateWidgetTitleUseCase>(),
                _container.Resolve<GetStatUseCase>()),
            _ => throw new AbandonedMutexException()
        };
    }
}