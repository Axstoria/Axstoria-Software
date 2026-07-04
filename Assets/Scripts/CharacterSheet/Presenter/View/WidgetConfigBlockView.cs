using System.ComponentModel;
using System.IO;
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
        [SerializeField] private ColorSwatchField borderColorSwatch;
        [SerializeField] private ColorSwatchField backgroundColorSwatch;
        [SerializeField] private TMP_Text backgroundFileLabel;
        [SerializeField] private Button clearBackgroundButton;

        private static readonly Color FileNameColor = new Color32(240, 240, 240, 255);
        private static readonly Color NoFileColor = new Color32(140, 140, 140, 255);

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
            borderColorSwatch.ValueChanged += SendBorderColor;
            backgroundColorSwatch.ValueChanged += SendBackgroundColor;

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
            borderColorSwatch.SetColorWithoutNotify(_widget.BorderColor);
            backgroundColorSwatch.SetColorWithoutNotify(_widget.BackgroundColor);

            string path = _widget.BackgroundImagePath;
            bool hasImage = !string.IsNullOrEmpty(path);
            backgroundFileLabel.text = hasImage ? Path.GetFileName(path) : "None";
            backgroundFileLabel.color = hasImage ? FileNameColor : NoFileColor;
            clearBackgroundButton.gameObject.SetActive(hasImage);
        }

        private void SendAppearance(AppearanceDTO dto)
        {
            _widget?.UpdateAppearanceCommand.Execute(dto);
        }

        private void SendBorderColor(Color color)
        {
            if (_widget == null) return;
            SendAppearance(new AppearanceDTO { BorderColor = new Color(color.r, color.g, color.b, _widget.BorderColor.a) });
        }

        private void SendBackgroundColor(Color color)
        {
            if (_widget == null) return;
            SendAppearance(new AppearanceDTO { BackgroundColor = new Color(color.r, color.g, color.b, _widget.BackgroundColor.a) });
        }

        private void OnDestroy()
        {
            if (_vm != null) _vm.PropertyChanged -= OnEditorPropertyChanged;
            if (_widget != null) _widget.PropertyChanged -= OnWidgetPropertyChanged;
        }
    }
}
