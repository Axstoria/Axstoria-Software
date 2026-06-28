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
        [SerializeField] private Button addPointGaugeWidgetButton;
        [SerializeField] private Button addTextWidgetButton;
        [SerializeField] private Button addCounterWidgetButton;
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
            if (UnityEngine.EventSystems.EventSystem.current != null) {
                GameObject currentSelected = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
                if (currentSelected != null && currentSelected.GetComponent<TMPro.TMP_InputField>() != null) return;
            }

            if (_vm != null && _vm.RemoveWidgetCommand.CanExecute(null)) {
                _vm.RemoveWidgetCommand.Execute(null);
            }
        }

        private void CreateBindings()
        {
            var bindingSet = this.CreateBindingSet<CharacterSheetEditorView, CharacterSheetEditorViewModel>();

            bindingSet.Bind(addPointGaugeWidgetButton)
                .For(v => v.onClick)
                .To(x => x.AddWidgetCommand)
                .CommandParameter(WidgetType.PointGauge);

            bindingSet.Bind(addTextWidgetButton)
                .For(v => v.onClick)
                .To(x => x.AddWidgetCommand)
                .CommandParameter(WidgetType.Text);
            
            bindingSet.Bind(addCounterWidgetButton)
                .For(v => v.onClick)
                .To(x => x.AddWidgetCommand)
                .CommandParameter(WidgetType.Counter);

            bindingSet.Bind(saveButton)
                .For(v => v.onClick)
                .To(x => x.SaveSheetCommand);

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