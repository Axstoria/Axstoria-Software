using App.Domain;
using App.Infrastructure;
using App.Presenter.ViewModel;
using Loxodon.Framework.Binding;
using Loxodon.Framework.Contexts;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace App
{
    public class AppBootstrapper : MonoBehaviour
    {
        void Awake()
        {
            var context = Context.GetApplicationContext();
            var container = context.GetContainer();
        
            BindingServiceBundle bundle = new BindingServiceBundle(container);
            bundle.Start();
        
            var navigationService = new UnityNavigationService();
            container.Register<INavigationService>(navigationService);
        
            container.Register(new HomeMenuViewModel(navigationService));
            container.Register(new EditionMenuViewModel(navigationService));
        
            DontDestroyOnLoad(this.gameObject);
            SceneManager.LoadScene("HomeMenuScene");
        }
    }
}