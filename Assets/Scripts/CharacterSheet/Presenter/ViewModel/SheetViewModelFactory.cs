using CharacterSheet.App.UseCase;
using CharacterSheet.Domain;
using Loxodon.Framework.Services;

namespace CharacterSheet.Presenter.ViewModel
{
    public class SheetViewModelFactory
    {
        private readonly IServiceContainer _container;

        public SheetViewModelFactory(IServiceContainer container)
        {
            _container = container;
        }

        public SheetViewModel Create(Sheet sheet)
        {
            return new SheetViewModel(
                sheet,
                _container.Resolve<WidgetViewModelFactory>(),
                _container.Resolve<UpdateSheetUseCase>(),
                _container.Resolve<AddWidgetUseCase>(),
                _container.Resolve<RemoveWidgetUseCase>(),
                _container.Resolve<BindStatToWidgetUseCase>(),
                _container.Resolve<UnbindStatUseCase>()
            );
        }
    }
}