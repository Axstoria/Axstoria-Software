using System;
using CharacterSheet.Presenter.View;
using HSVPicker;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace EditorShell.Presenter.View
{
    // Bridges the UI Toolkit settings panel to the real (uGUI) HSVPicker color picker package —
    // ColorPicker is a Canvas/RectTransform widget, and this panel's scene has no Canvas of its own,
    // so one is created at runtime instead of requiring manual scene wiring. A single instance is
    // shared across every color swatch in the panel (terrain, light, ambient, ...) since only one
    // picker can be open at a time.
    public class ColorPickerOverlay : MonoBehaviour
    {
        private const string PickerPrefabResourcePath = "ColorPickerPanel";

        private ColorPickerPopup _popup;

        private void Awake()
        {
            EnsureEventSystem();
            _popup = CreatePopup();
        }

        public void Open(Color current, VisualElement anchor, Action<Color> onChanged)
        {
            if (_popup == null) return;

            Rect bound = anchor.worldBound;
            Vector2 screenPoint = new Vector2(bound.xMin, Screen.height - bound.yMax);
            _popup.Open(current, screenPoint, onChanged);
        }

        private static void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null) return;

            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        private ColorPickerPopup CreatePopup()
        {
            var canvasGO = new GameObject("ColorPickerCanvas",
                typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGO.transform.SetParent(transform, false);

            Canvas canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;

            // Parent before adding the component so ColorPickerPopup.Awake() can find the Canvas above it.
            var popupGO = new GameObject("ColorPickerPopup");
            popupGO.transform.SetParent(canvasGO.transform, false);
            var popup = popupGO.AddComponent<ColorPickerPopup>();

            var prefab = Resources.Load<ColorPicker>(PickerPrefabResourcePath);
            if (prefab == null)
                Debug.LogError($"[ColorPickerOverlay] Could not load '{PickerPrefabResourcePath}' from Resources.");
            popup.SetPickerPrefab(prefab);

            return popup;
        }
    }
}
