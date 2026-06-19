using CharacterSheet.Domain;
using CharacterSheet.Presenter.ViewModel;
using Loxodon.Framework.Contexts;
using UnityEngine;
using UnityEngine.UI;

namespace CharacterSheet.Presenter.View
{
    public class WidgetPreviewButton : MonoBehaviour
    {
        [SerializeField] private WidgetType widgetType;
        [SerializeField] private Button button;
        
        private CharacterSheetEditorViewModel viewModel;

        public void Start()
        {
            var container = Context.GetApplicationContext().GetContainer();
            viewModel = container.Resolve<CharacterSheetEditorViewModel>();
            
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnButtonClick);
        }

        private void OnButtonClick()
        {
            if (viewModel != null && viewModel.AddWidgetCommand.CanExecute(widgetType))
                viewModel.AddWidgetCommand.Execute(widgetType);
        }
    }
}
