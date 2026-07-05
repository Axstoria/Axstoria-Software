using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace SceneEditor.Presenter.View
{
    public class ColorPickerPopupController : MonoBehaviour
    {
        private static readonly string[] PresetColors =
        {
            "#E53935", "#D81B60", "#8E24AA", "#5E35B1", "#3949AB", "#1E88E5",
            "#039BE5", "#00ACC1", "#00897B", "#43A047", "#7CB342", "#C0CA33",
            "#FDD835", "#FFB300", "#FB8C00", "#F4511E", "#6D4C41", "#9E9E9E",
            "#546E7A", "#FFFFFF", "#BDBDBD", "#000000", "#EF5350", "#26A69A",
        };

        private VisualElement _root;
        private VisualElement _popup;
        private TextField _hexField;
        private VisualElement _preview;
        private VisualElement _swatchGrid;
        private Action<string> _onColorPicked;

        public void Init(VisualElement root)
        {
            VisualTreeAsset asset = Resources.Load<VisualTreeAsset>("MetadataPopup/ColorPickerPopup");
            VisualElement instance = asset.Instantiate();
            root.Add(instance);
            _root = root;

            _popup      = instance.Q<VisualElement>("color-picker-popup");
            _hexField   = instance.Q<TextField>("color-hex-field");
            _preview    = instance.Q<VisualElement>("color-preview");
            _swatchGrid = instance.Q<VisualElement>("color-swatch-grid");

            foreach (string hex in PresetColors)
            {
                var cell = new VisualElement();
                cell.AddToClassList("color-picker-cell");
                if (ColorUtility.TryParseHtmlString(hex, out Color c))
                    cell.style.backgroundColor = c;
                cell.RegisterCallback<ClickEvent>(_ => PickColor(hex));
                _swatchGrid.Add(cell);
            }

            _hexField.RegisterCallback<KeyDownEvent>(evt =>
            {
                if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
                    PickColor(_hexField.value);
            });

            _root.RegisterCallback<PointerDownEvent>(evt =>
            {
                if (_popup.style.display == DisplayStyle.None) return;
                if (!_popup.worldBound.Contains(evt.position))
                    Close();
            }, TrickleDown.TrickleDown);

            _popup.style.display = DisplayStyle.None;
        }

        public void Open(VisualElement anchor, string currentHex, Action<string> onColorPicked)
        {
            _onColorPicked = onColorPicked;
            _hexField.SetValueWithoutNotify(currentHex);
            UpdatePreview(currentHex);

            Rect bound = anchor.worldBound;
            _popup.style.left = bound.xMin;
            _popup.style.top  = bound.yMax + 4;
            _popup.style.display = DisplayStyle.Flex;
            _popup.BringToFront();
        }

        public void Close()
        {
            _popup.style.display = DisplayStyle.None;
            _onColorPicked = null;
        }

        private void PickColor(string hex)
        {
            if (!ColorUtility.TryParseHtmlString(hex, out Color c)) return;
            hex = "#" + ColorUtility.ToHtmlStringRGB(c);
            UpdatePreview(hex);
            _hexField.SetValueWithoutNotify(hex);
            _onColorPicked?.Invoke(hex);
            Close();
        }

        private void UpdatePreview(string hex)
        {
            if (ColorUtility.TryParseHtmlString(hex, out Color c))
                _preview.style.backgroundColor = c;
        }
    }
}
