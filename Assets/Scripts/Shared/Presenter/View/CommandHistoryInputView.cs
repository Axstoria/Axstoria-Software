using Loxodon.Framework.Contexts;
using MapEditor.Presenter.ViewModels;
using UnityEngine;

namespace Shared.Presenter.View
{
    public class CommandHistoryInputView : MonoBehaviour
    {
        private MapEditorViewModel _vm;

        private void Start()
        {
            _vm = Context.GetApplicationContext()
                         .GetContainer()
                         .Resolve<MapEditorViewModel>();

            if (_vm == null)
            {
                Debug.LogError("[CommandHistoryInputView] MapEditorViewModel not found.");
                enabled = false;
            }
        }

        private void Update()
        {
            bool ctrl  = Input.GetKey(KeyCode.LeftControl)  || Input.GetKey(KeyCode.RightControl);
            bool shift = Input.GetKey(KeyCode.LeftShift)    || Input.GetKey(KeyCode.RightShift);

            if (ctrl && !shift && Input.GetKeyDown(KeyCode.Z))                                 _vm.Undo();
            if (ctrl && (Input.GetKeyDown(KeyCode.Y) || shift && Input.GetKeyDown(KeyCode.Z))) _vm.Redo();
        }
    }
}
