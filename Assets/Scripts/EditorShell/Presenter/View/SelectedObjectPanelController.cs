using UnityEngine;
using UnityEngine.UIElements;

namespace EditorShell.Presenter.View
{
    public class SelectedObjectPanelController : MonoBehaviour
    {
        private const float MinPanelHeight = 90f;
        private const float MinListHeight = 26f;
        private const float DefaultRatio = 0.5f;

        private static readonly Vector2 ResizeCursorHotspot = new Vector2(16, 16);

        private Texture2D _resizeCursor;

        private VisualElement _panel;
        private VisualElement _handle;
        private VisualElement _outlinerPane;
        private VisualElement _outlinerHeader;
        private VisualElement _outlinerSearch;

        private bool _isDragging;
        private float _dragStartY;
        private float _dragStartHeight;

        public void Init(VisualElement root, Texture2D resizeCursor)
        {
            _resizeCursor   = resizeCursor;
            _panel          = root.Q<VisualElement>("selected-object");
            _handle         = root.Q<VisualElement>("selected-object-resize-handle");
            _outlinerPane   = root.Q<VisualElement>("outliner-pane");
            _outlinerHeader = root.Q<VisualElement>("outliner-header");
            _outlinerSearch = root.Q<VisualElement>("outliner-search");

            if (_panel == null || _handle == null || _outlinerPane == null) return;

            _handle.RegisterCallback<PointerDownEvent>(OnResizePointerDown);
            _handle.RegisterCallback<PointerMoveEvent>(OnResizePointerMove);
            _handle.RegisterCallback<PointerUpEvent>(OnResizePointerUp);
            _handle.RegisterCallback<MouseEnterEvent>(_ => UnityEngine.Cursor.SetCursor(_resizeCursor, ResizeCursorHotspot, CursorMode.ForceSoftware));
            _handle.RegisterCallback<MouseLeaveEvent>(_ => UnityEngine.Cursor.SetCursor(null, Vector2.zero, CursorMode.ForceSoftware));

            _outlinerPane.RegisterCallback<GeometryChangedEvent>(OnOutlinerGeometryChanged);
        }

        private void OnOutlinerGeometryChanged(GeometryChangedEvent evt)
        {
            float paneHeight = _outlinerPane.resolvedStyle.height;
            if (paneHeight <= 0f) return;

            _outlinerPane.UnregisterCallback<GeometryChangedEvent>(OnOutlinerGeometryChanged);
            _panel.style.height = Mathf.Clamp(paneHeight * DefaultRatio, MinPanelHeight, GetMaxPanelHeight());
        }

        private float GetMaxPanelHeight()
        {
            float paneHeight   = _outlinerPane.resolvedStyle.height;
            float headerHeight = _outlinerHeader != null ? _outlinerHeader.resolvedStyle.height : 0f;
            float searchHeight = _outlinerSearch != null ? _outlinerSearch.resolvedStyle.height : 0f;
            float handleHeight = _handle.resolvedStyle.height;
            return paneHeight - headerHeight - searchHeight - handleHeight - MinListHeight;
        }

        private void OnResizePointerDown(PointerDownEvent evt)
        {
            _isDragging     = true;
            _dragStartY     = evt.position.y;
            _dragStartHeight = _panel.resolvedStyle.height;
            _handle.CapturePointer(evt.pointerId);
        }

        private void OnResizePointerMove(PointerMoveEvent evt)
        {
            if (!_isDragging) return;
            float delta = evt.position.y - _dragStartY;
            _panel.style.height = Mathf.Clamp(_dragStartHeight - delta, MinPanelHeight, GetMaxPanelHeight());
        }

        private void OnResizePointerUp(PointerUpEvent evt)
        {
            if (!_isDragging) return;
            _isDragging = false;
            _handle.ReleasePointer(evt.pointerId);
        }
    }
}
