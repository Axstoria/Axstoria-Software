using App.Domain;
using Loxodon.Framework.Commands;
using Loxodon.Framework.ViewModels;

namespace App.Presenter.ViewModel
{
    public class HomeMenuViewModel : ViewModelBase
    {
        private readonly INavigationService _navigation;

        public ICommand GoToGame { get; }
        public ICommand GoToEdition { get; }

        public HomeMenuViewModel(INavigationService navigation)
        {
            _navigation = navigation;

            GoToGame = new SimpleCommand(() => _navigation.LoadScene(SceneNames.GameMenu));
            GoToEdition = new SimpleCommand(() => _navigation.LoadScene(SceneNames.EditionMenu));
        }
    }
}