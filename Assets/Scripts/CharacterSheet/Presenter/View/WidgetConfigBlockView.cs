using System.ComponentModel;
using CharacterSheet.App.DTO;
using CharacterSheet.Presenter.ViewModel;
using Loxodon.Framework.Contexts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CharacterSheet.Presenter.View
{
    public class WidgetConfigBlockView : MonoBehaviour
    {
        [SerializeField] private TMP_InputField titleField;
        [SerializeField] private Toggle borderToggle;
        [SerializeField] private Slider thicknessSlider;
        [SerializeField] private Slider borderColorR;
        [SerializeField] private Slider borderColorG;
        [SerializeField] private Slider borderColorB;
        [SerializeField] private Slider backgroundColorR;
        [SerializeField] private Slider backgroundColorG;
        [SerializeField] private Slider backgroundColorB;

        private CharacterSheetEditorViewModel _vm;
        private WidgetViewModel _widget;

        private void Start()
        {
            _vm = Context.GetApplicationContext().GetContainer().Resolve<CharacterSheetEditorViewModel>();
            if (_vm == null) return;

            _vm.PropertyChanged += OnEditorPropertyChanged;

            titleField.onEndEdit.AddListener(v => _widget?.UpdateTitleCommand.Execute(v));
            borderToggle.onValueChanged.AddListener(v => SendAppearance(new AppearanceDTO { HasBorder = v }));
            thicknessSlider.onValueChanged.AddListener(v => SendAppearance(new AppearanceDTO { BorderThickness = v }));
            borderColorR.onValueChanged.AddListener(_ => SendBorderColor());
            borderColorG.onValueChanged.AddListener(_ => SendBorderColor());
            borderColorB.onValueChanged.AddListener(_ => SendBorderColor());
            backgroundColorR.onValueChanged.AddListener(_ => SendBackgroundColor());
            backgroundColorG.onValueChanged.AddListener(_ => SendBackgroundColor());
            backgroundColorB.onValueChanged.AddListener(_ => SendBackgroundColor());

            Bind(_vm.SelectedWidget);
        }

        private void OnEditorPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == nameof(CharacterSheetEditorViewModel.SelectedWidget))
                Bind(_vm.SelectedWidget);
        }

        private void Bind(WidgetViewModel widget)
        {
            if (_widget != null) _widget.PropertyChanged -= OnWidgetPropertyChanged;
            _widget = widget;
            if (_widget != null) _widget.PropertyChanged += OnWidgetPropertyChanged;
            Refresh();
        }

        private void OnWidgetPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Refresh();
        }

        private void Refresh()
        {
            if (_widget == null) return;
            titleField.SetTextWithoutNotify(_widget.Title);
            borderToggle.SetIsOnWithoutNotify(_widget.HasBorder);
            thicknessSlider.SetValueWithoutNotify(_widget.BorderThickness);
            borderColorR.SetValueWithoutNotify(_widget.BorderColor.r);
            borderColorG.SetValueWithoutNotify(_widget.BorderColor.g);
            borderColorB.SetValueWithoutNotify(_widget.BorderColor.b);
            backgroundColorR.SetValueWithoutNotify(_widget.BackgroundColor.r);
            backgroundColorG.SetValueWithoutNotify(_widget.BackgroundColor.g);
            backgroundColorB.SetValueWithoutNotify(_widget.BackgroundColor.b);
        }

        private void SendAppearance(AppearanceDTO dto)
        {
            _widget?.UpdateAppearanceCommand.Execute(dto);
        }

        private void SendBorderColor()
        {
            if (_widget == null) return;
            var color = new Color(borderColorR.value, borderColorG.value, borderColorB.value, _widget.BorderColor.a);
            SendAppearance(new AppearanceDTO { BorderColor = color });
        }

        private void SendBackgroundColor()
        {
            if (_widget == null) return;
            var color = new Color(backgroundColorR.value, backgroundColorG.value, backgroundColorB.value, _widget.BackgroundColor.a);
            SendAppearance(new AppearanceDTO { BackgroundColor = color });
        }

        private void OnDestroy()
        {
            if (_vm != null) _vm.PropertyChanged -= OnEditorPropertyChanged;
            if (_widget != null) _widget.PropertyChanged -= OnWidgetPropertyChanged;
        }
    }
}
