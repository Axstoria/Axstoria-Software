using System.Threading;
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

        public WidgetViewModel Create(SheetWidget widget) => widget switch
        {
            PointGaugeWidget g => new PointGaugeViewModel(g,
                _container.Resolve<BindStatToWidgetUseCase>(),
                _container.Resolve<UpdateWidgetAppearanceUseCase>(),
                _container.Resolve<UpdateWidgetLayoutUseCase>(),
                _container.Resolve<UpdateWidgetTitleUseCase>(),
                _container.Resolve<GetStatUseCase>(),
                _container.Resolve<UpdatePointGaugeWidgetUseCase>()),
            TextWidget t => new TextWidgetViewModel(t,
                _container.Resolve<BindStatToWidgetUseCase>(),
                _container.Resolve<UpdateWidgetAppearanceUseCase>(),
                _container.Resolve<UpdateWidgetLayoutUseCase>(),
                _container.Resolve<UpdateWidgetTitleUseCase>(),
                _container.Resolve<GetStatUseCase>(),
                _container.Resolve<UpdateTextWidgetUseCase>()),
            CounterWidget c => new CounterWidgetViewModel(c,
                _container.Resolve<BindStatToWidgetUseCase>(),
                _container.Resolve<UpdateWidgetAppearanceUseCase>(),
                _container.Resolve<UpdateWidgetLayoutUseCase>(),
                _container.Resolve<UpdateWidgetTitleUseCase>(),
                _container.Resolve<GetStatUseCase>()),
            _ => throw new AbandonedMutexException()
        };
    }
}