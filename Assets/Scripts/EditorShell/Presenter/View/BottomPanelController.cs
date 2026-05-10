using UnityEngine;
using UnityEngine.UIElements;

namespace EditorShell.Presenter.View
{
    public class BottomPanelController : MonoBehaviour
    {
        private const float MinContentHeight = 80f;
        private const float MarginFromViewSwitcher = 10f;

        [SerializeField] private Texture2D _resizeCursor;
        private static readonly Vector2 ResizeCursorHotspot = new Vector2(16, 16);

        private VisualElement _root;
        private VisualElement _content;
        private VisualElement _resizeHandle;
        private VisualElement _toggleBar;
        private VisualElement _viewSwitcher;
        private Button _toggleBtn;
        private bool _isExpanded = false;
        private bool _isDragging = false;
        private float _dragStartY;
        private float _dragStartHeight;

        public void Init(VisualElement root)
        {
            _root = root;
            _content = root.Q<VisualElement>("bottom-panel-content");
            _resizeHandle = root.Q<VisualElement>("bottom-panel-resize-handle");
            _toggleBar = root.Q<VisualElement>("bottom-panel-toggle-bar");
            _viewSwitcher = root.Q<VisualElement>("view-switcher");
            _toggleBtn = root.Q<Button>("bottom-panel-toggle-btn");

            _toggleBtn.clicked += OnToggleClicked;
            _resizeHandle.RegisterCallback<PointerDownEvent>(OnResizePointerDown);
            _resizeHandle.RegisterCallback<PointerMoveEvent>(OnResizePointerMove);
            _resizeHandle.RegisterCallback<PointerUpEvent>(OnResizePointerUp);
            _resizeHandle.RegisterCallback<MouseEnterEvent>(_ => { if (_isExpanded) UnityEngine.Cursor.SetCursor(_resizeCursor, ResizeCursorHotspot, CursorMode.ForceSoftware); });
            _resizeHandle.RegisterCallback<MouseLeaveEvent>(_ => UnityEngine.Cursor.SetCursor(null, Vector2.zero, CursorMode.ForceSoftware));
        }

        private void OnToggleClicked()
        {
            _isExpanded = !_isExpanded;

            if (_isExpanded)
            {
                _content.AddToClassList("bottom-panel__content--visible");
                _toggleBtn.AddToClassList("bottom-panel__toggle-btn--expanded");
            }
            else
            {
                _content.RemoveFromClassList("bottom-panel__content--visible");
                _toggleBtn.RemoveFromClassList("bottom-panel__toggle-btn--expanded");
            }
        }

        private void OnResizePointerDown(PointerDownEvent evt)
        {
            if (!_isExpanded) return;
            _isDragging = true;
            _dragStartY = evt.position.y;
            _dragStartHeight = _content.resolvedStyle.height;
            _resizeHandle.CapturePointer(evt.pointerId);
        }

        private float GetMaxContentHeight()
        {
            float availableHeight = _root.resolvedStyle.height;
            float panelChrome = _resizeHandle.resolvedStyle.height + _toggleBar.resolvedStyle.height;
            float viewSwitcherBottom = _viewSwitcher.layout.yMax;
            return availableHeight - panelChrome - viewSwitcherBottom - MarginFromViewSwitcher;
        }

        private void OnResizePointerMove(PointerMoveEvent evt)
        {
            if (!_isDragging) return;
            float delta = evt.position.y - _dragStartY;
            _content.style.height = Mathf.Clamp(_dragStartHeight - delta, MinContentHeight, GetMaxContentHeight());
        }

        private void OnResizePointerUp(PointerUpEvent evt)
        {
            if (!_isDragging) return;
            _isDragging = false;
            _resizeHandle.ReleasePointer(evt.pointerId);
        }
    }
}
