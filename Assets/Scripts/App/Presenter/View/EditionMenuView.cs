using App.Presenter.ViewModel;
using Loxodon.Framework.Binding;
using Loxodon.Framework.Contexts;
using Loxodon.Framework.Views;
using UnityEngine.UI;

namespace App.Presenter.View
{
    public class EditionMenuView : UIView
    {
        public Button preloadShaderButton;

        private void Start() 
        {
            var vm = Context.GetApplicationContext().GetContainer().Resolve<EditionMenuViewModel>();
        
            var bindingSet = this.CreateBindingSet(vm);
            bindingSet.Bind(preloadShaderButton).For(v => v.onClick).To(m => m.GoToPreload);
            bindingSet.Build();
        }
    }
}