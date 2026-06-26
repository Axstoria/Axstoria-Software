using System.ComponentModel;
using CharacterSheet.Domain;
using CharacterSheet.Presenter.ViewModel;
using Loxodon.Framework.Binding;
using Loxodon.Framework.Contexts;
using Loxodon.Framework.Views;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace CharacterSheet.Presenter.View
{
    public class CharacterSheetEditorView : UIView
    {
        [SerializeField] private Transform widgetContainer;
        [SerializeField] private Button saveButton;
        [SerializeField] private Button addWidgetButton;
        [SerializeField] private GameObject sheetPrefab;
        private GameInputs _inputs;

        private CharacterSheetEditorViewModel _vm;

        protected override void Awake()
        {
            base.Awake();

            _vm = Context.GetApplicationContext().GetContainer().Resolve<CharacterSheetEditorViewModel>();
            this.SetDataContext(_vm);

            var sheetViewModel = _vm.CurrentSheet;
            if (sheetViewModel != null) {
                var go = Instantiate(sheetPrefab, widgetContainer);
                var view = go.GetComponent<SheetView>();
                view.SetDataContext(sheetViewModel);
                view.Initialize(sheetViewModel);
            }

            CreateBindings();

            _inputs = new GameInputs();
            _inputs.Editor.Enable();
            _inputs.Editor.DeleteWidget.performed += OnDeleteWidgetPerformed;
        }

        private void OnDeleteWidgetPerformed(InputAction.CallbackContext context)
        {
            if (_vm != null && _vm.RemoveWidgetCommand.CanExecute(null)) {
                _vm.RemoveWidgetCommand.Execute(null);
            }
        }

        private void CreateBindings()
        {
            var bindingSet = this.CreateBindingSet<CharacterSheetEditorView, CharacterSheetEditorViewModel>();

            bindingSet.Bind(addWidgetButton)
                .For(v => v.onClick)
                .To(x => x.AddWidgetCommand)
                .CommandParameter(WidgetType.PointGauge);

            bindingSet.Bind(saveButton)
                .For(v => v.onClick)
                .To(x => x.SaveSheetCommand);

            /*bindingSet.Bind(addStatButton)
                .For(v => v.onClick)
                .To(x => x.AddStatCommand);*/

            bindingSet.Build();
        }

        protected override void OnDestroy()
        {
            if (_inputs != null) {
                _inputs.Editor.DeleteWidget.performed -= OnDeleteWidgetPerformed;
                _inputs.Editor.Disable();
                _inputs.Dispose();
            }

            base.OnDestroy();
            _vm = null;
        }
    }
}