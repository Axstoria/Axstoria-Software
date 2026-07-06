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
            RectTransform rt = SpawnPicker(current, onChanged);
            rt.pivot = new Vector2(1f, 1f);

            var corners = new Vector3[4];
            anchor.GetWorldCorners(corners);
            rt.position = corners[0];
            ClampToCanvas(rt);
        }

        // For anchors that aren't uGUI RectTransforms (e.g. a UI Toolkit VisualElement's screen-space bounds).
        public void Open(Color current, Vector2 screenPosition, Action<Color> onChanged)
        {
            RectTransform rt = SpawnPicker(current, onChanged);
            rt.pivot = new Vector2(0f, 1f);

            UnityEngine.Camera cam = _canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _canvas.worldCamera;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                (RectTransform)_canvas.transform, screenPosition, cam, out Vector2 localPoint);
            rt.localPosition = localPoint;
            ClampToCanvas(rt);
        }

        // Lets code that instantiates this popup at runtime (without an Inspector-wired prefab) supply one.
        public void SetPickerPrefab(ColorPicker prefab) => pickerPrefab = prefab;

        private RectTransform SpawnPicker(Color current, Action<Color> onChanged)
        {
            Close();
            _onChanged = onChanged;

            _blocker = CreateBlocker();

            _picker = Instantiate(pickerPrefab, _canvas.transform);
            _picker.CurrentColor = current;
            _picker.onValueChanged.AddListener(HandleValueChanged);

            return (RectTransform)_picker.transform;
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
