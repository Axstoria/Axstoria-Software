using Controler.Editor.ViewModels;
using Loxodon.Framework.Contexts;
using UnityEngine;

namespace Controler.Editor.Views
{
    /// <summary>
    /// Intercepts Ctrl+Z / Ctrl+Y keyboard shortcuts and delegates to the ViewModel.
    /// No logic lives here — just input forwarding.
    /// </summary>
    public class CommandHistoryInputView : MonoBehaviour
    {
        private MapEditorViewModel _vm;

        private void Start()
        {
            _vm = Context.GetApplicationContext()
                         .GetContainer()
                         .Resolve<MapEditorViewModel>();
        }

        private void Update()
        {
            bool ctrl  = Input.GetKey(KeyCode.LeftControl)  || Input.GetKey(KeyCode.RightControl);
            bool shift = Input.GetKey(KeyCode.LeftShift)    || Input.GetKey(KeyCode.RightShift);

            if (ctrl && !shift && Input.GetKeyDown(KeyCode.Z))                               _vm.Undo();
            if (ctrl && (Input.GetKeyDown(KeyCode.Y) || shift && Input.GetKeyDown(KeyCode.Z))) _vm.Redo();
        }
    }
}
