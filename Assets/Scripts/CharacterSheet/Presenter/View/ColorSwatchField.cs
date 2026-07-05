using System;
using UnityEngine;
using UnityEngine.UI;

namespace CharacterSheet.Presenter.View
{
    public class ColorSwatchField : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Image swatch;
        [SerializeField] private ColorPickerPopup popup;

        private Color _current = Color.white;

        public event Action<Color> ValueChanged;

        private void Awake()
        {
            if (button != null)
                button.onClick.AddListener(OpenPicker);
        }

        public void SetColorWithoutNotify(Color color)
        {
            _current = color;
            if (swatch != null)
                swatch.color = new Color(color.r, color.g, color.b, 1f);
        }

        private void OpenPicker()
        {
            if (popup == null) return;
            popup.Open(_current, (RectTransform)transform, color =>
            {
                SetColorWithoutNotify(color);
                ValueChanged?.Invoke(color);
            });
        }
    }
}
