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
    public class SheetConfigBlockView : MonoBehaviour
    {
        [SerializeField] private Toggle borderToggle;
        [SerializeField] private Slider thicknessSlider;
        [SerializeField] private ColorSwatchField borderColorSwatch;
        [SerializeField] private ColorSwatchField backgroundColorSwatch;
        [SerializeField] private TMP_Text backgroundFileLabel;
        [SerializeField] private Button clearBackgroundButton;
        [SerializeField] private Button importBackgroundButton;

        private static readonly Color FileNameColor = new Color32(240, 240, 240, 255);
        private static readonly Color NoFileColor = new Color32(140, 140, 140, 255);

        private CharacterSheetEditorViewModel _vm;
        private SheetViewModel _sheet;

        private void Start()
        {
            _vm = Context.GetApplicationContext().GetContainer().Resolve<CharacterSheetEditorViewModel>();
            if (_vm == null) return;

            _vm.PropertyChanged += OnEditorPropertyChanged;

            borderToggle.onValueChanged.AddListener(v => SendAppearance(new AppearanceDTO { HasBorder = v }));
            thicknessSlider.onValueChanged.AddListener(v => SendAppearance(new AppearanceDTO { BorderThickness = v }));
            borderColorSwatch.ValueChanged += SendBorderColor;
            backgroundColorSwatch.ValueChanged += SendBackgroundColor;
            importBackgroundButton.onClick.AddListener(() => _sheet?.SelectBackgroundCommand.Execute(null));
            clearBackgroundButton.onClick.AddListener(ClearBackground);

            Bind(_vm.CurrentSheet);
        }

        private void OnEditorPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == nameof(CharacterSheetEditorViewModel.CurrentSheet))
                Bind(_vm.CurrentSheet);
        }

        private void Bind(SheetViewModel sheet)
        {
            if (_sheet != null) _sheet.PropertyChanged -= OnSheetPropertyChanged;
            _sheet = sheet;
            if (_sheet != null) _sheet.PropertyChanged += OnSheetPropertyChanged;
            Refresh();
        }

        private void OnSheetPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Refresh();
        }

        private void Refresh()
        {
            if (_sheet == null) return;
            borderToggle.SetIsOnWithoutNotify(_sheet.HasBorder);
            thicknessSlider.SetValueWithoutNotify(_sheet.BorderThickness);
            borderColorSwatch.SetColorWithoutNotify(_sheet.BorderColor);
            backgroundColorSwatch.SetColorWithoutNotify(_sheet.BackgroundColor);

            string path = _sheet.RuntimeSheet != null ? _sheet.RuntimeSheet.BackgroundImagePath : null;
            bool hasImage = !string.IsNullOrEmpty(path);
            backgroundFileLabel.text = hasImage ? Path.GetFileName(path) : "None";
            backgroundFileLabel.color = hasImage ? FileNameColor : NoFileColor;
            clearBackgroundButton.gameObject.SetActive(hasImage);
        }

        private void ClearBackground()
        {
            if (_sheet?.RuntimeSheet == null) return;
            _sheet.RuntimeSheet.BackgroundImagePath = null;
        }

        private void SendAppearance(AppearanceDTO dto)
        {
            _sheet?.UpdateAppearanceCommand.Execute(dto);
        }

        private void SendBorderColor(Color color)
        {
            if (_sheet == null) return;
            SendAppearance(new AppearanceDTO { BorderColor = new Color(color.r, color.g, color.b, _sheet.BorderColor.a) });
        }

        private void SendBackgroundColor(Color color)
        {
            if (_sheet == null) return;
            SendAppearance(new AppearanceDTO { BackgroundColor = new Color(color.r, color.g, color.b, _sheet.BackgroundColor.a) });
        }

        private void OnDestroy()
        {
            if (_vm != null) _vm.PropertyChanged -= OnEditorPropertyChanged;
            if (_sheet != null) _sheet.PropertyChanged -= OnSheetPropertyChanged;
        }
    }
}
