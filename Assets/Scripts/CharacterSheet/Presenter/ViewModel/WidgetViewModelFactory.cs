using System.Threading;
using CharacterSheet.App.UseCase;
using CharacterSheet.Domain;
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
            _container.Resolve<UpdateWidgetAppearanceUseCase>(),
            _container.Resolve<UpdateWidgetLayoutUseCase>(),
            _container.Resolve<UpdatePointGaugeWidgetUseCase>())
            ,
            _ => throw new AbandonedMutexException()
        };
    }
}