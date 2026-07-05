using System;
using HSVPicker;
using UnityEngine;
using UnityEngine.UI;

namespace CharacterSheet.Presenter.View
{
    public class ColorPickerPopup : MonoBehaviour
    {
        [SerializeField] private ColorPicker pickerPrefab;

        private Canvas _canvas;
        private ColorPicker _picker;
        private GameObject _blocker;
        private Action<Color> _onChanged;

        private void Awake()
        {
            _canvas = GetComponentInParent<Canvas>();
        }

        public void Open(Color current, RectTransform anchor, Action<Color> onChanged)
        {
            Close();
            _onChanged = onChanged;

            _blocker = CreateBlocker();

            _picker = Instantiate(pickerPrefab, _canvas.transform);
            var rt = (RectTransform)_picker.transform;
            rt.pivot = new Vector2(1f, 1f);

            var corners = new Vector3[4];
            anchor.GetWorldCorners(corners);
            rt.position = corners[0];
            ClampToCanvas(rt);

            _picker.CurrentColor = current;
            _picker.onValueChanged.AddListener(HandleValueChanged);
        }

        public void Close()
        {
            if (_picker != null) Destroy(_picker.gameObject);
            if (_blocker != null) Destroy(_blocker);
            _picker = null;
            _blocker = null;
            _onChanged = null;
        }

        private void HandleValueChanged(Color color)
        {
            _onChanged?.Invoke(color);
        }

        private void ClampToCanvas(RectTransform rt)
        {
            var canvasRt = (RectTransform)_canvas.transform;
            Vector2 local = canvasRt.InverseTransformPoint(rt.position);
            Vector2 size = rt.rect.size;
            Rect area = canvasRt.rect;

            local.x = Mathf.Clamp(local.x, area.xMin + size.x, area.xMax);
            local.y = Mathf.Clamp(local.y, area.yMin + size.y, area.yMax);
            rt.position = canvasRt.TransformPoint(local);
        }

        private GameObject CreateBlocker()
        {
            var go = new GameObject("ColorPickerBlocker", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            var rt = (RectTransform)go.transform;
            rt.SetParent(_canvas.transform, false);
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;

            go.GetComponent<Image>().color = Color.clear;

            var button = go.GetComponent<Button>();
            button.transition = Selectable.Transition.None;
            button.onClick.AddListener(Close);
            return go;
        }
    }
}
