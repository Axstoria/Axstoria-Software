using App.Presenter.ViewModel;
using Loxodon.Framework.Binding;
using Loxodon.Framework.Contexts;
using Loxodon.Framework.Views;
using UnityEngine;
using UnityEngine.UI;


namespace App.Presenter.View
{
    public class HomeMenuView : UIView
    {
        public Button gameButton;
        public Button editionButton;

        private void Start() 
        {
            var container = Context.GetApplicationContext().GetContainer();
            var vm = container.Resolve<HomeMenuViewModel>();

            if (vm == null) {
                Debug.LogError("Cannot find HomeMenuViewModel !");
                return;
            }
            
            var bindingSet = this.CreateBindingSet(vm);
            bindingSet.Bind(gameButton).For(v => v.onClick).To(x => x.GoToGame);
            bindingSet.Bind(editionButton).For(v => v.onClick).To(x => x.GoToEdition);
            bindingSet.Build();
        }
    }
}
