using App.Domain;
using Loxodon.Framework.Commands;
using Loxodon.Framework.ViewModels;

namespace App.Presenter.ViewModel
{
    public class EditionMenuViewModel : ViewModelBase
    {
        private readonly INavigationService _navigation;
        public ICommand GoToPreload { get; }

        public EditionMenuViewModel(INavigationService navigation)
        {
            _navigation = navigation;
            GoToPreload = new SimpleCommand(() => _navigation.LoadScene(SceneNames.BuildMenu));
        }
    }
}