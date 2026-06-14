using UnityEngine;
using UnityEngine.UIElements;

namespace EditorShell.Presenter.View
{
    public class SplitLayoutController : MonoBehaviour
    {
        private const float SettingsMin = 120f;
        private const float OutlinerMin = 120f;
        private const float MapMin      = 200f;
        private const float HandleWidth = 8f;

        private static readonly Vector2 ResizeCursorHotspot = new Vector2(16, 16);

        private Texture2D     _resizeCursor;
        private bool          _ownsCursor;
        private VisualElement _root;
        private VisualElement _settings;
        private VisualElement _outliner;

        private bool          _dragging;
        private float         _dragStartX;
        private float         _dragStartWidth;
        private VisualElement _dragTarget;
        private VisualElement _dragHandle;

        public void Init(VisualElement root, Texture2D resizeCursor)
        {
            // The shared cursor texture is vertical (north-south); rotate it 90° for
            // the horizontal pane handles.
            if (resizeCursor != null && resizeCursor.isReadable)
            {
                _resizeCursor = RotateClockwise90(resizeCursor);
                _ownsCursor   = true;
            }
            else
            {
                _resizeCursor = resizeCursor;
            }

            _root     = root.Q<VisualElement>("editor-split-root");
            _settings = root.Q<VisualElement>("settings-pane");
            _outliner = root.Q<VisualElement>("outliner-pane");

            VisualElement handleLeft  = root.Q<VisualElement>("split-handle-left");
            VisualElement handleRight = root.Q<VisualElement>("split-handle-right");

            if (handleLeft != null && _settings != null)
                RegisterHandle(handleLeft, _settings, false);
            if (handleRight != null && _outliner != null)
                RegisterHandle(handleRight, _outliner, true);
        }

        private void RegisterHandle(VisualElement handle, VisualElement pane, bool isRight)
        {
            handle.RegisterCallback<PointerDownEvent>(evt => OnPointerDown(evt, handle, pane));
            handle.RegisterCallback<PointerMoveEvent>(evt => OnPointerMove(evt, isRight));
            handle.RegisterCallback<PointerUpEvent>(OnPointerUp);

            if (_resizeCursor != null)
            {
                handle.RegisterCallback<MouseEnterEvent>(_ =>
                    UnityEngine.Cursor.SetCursor(_resizeCursor, ResizeCursorHotspot, CursorMode.ForceSoftware));
                handle.RegisterCallback<MouseLeaveEvent>(_ =>
                {
                    if (!_dragging) UnityEngine.Cursor.SetCursor(null, Vector2.zero, CursorMode.ForceSoftware);
                });
            }
        }

        private void OnPointerDown(PointerDownEvent evt, VisualElement handle, VisualElement pane)
        {
            _dragging       = true;
            _dragTarget     = pane;
            _dragHandle     = handle;
            _dragStartX     = evt.position.x;
            _dragStartWidth = pane.resolvedStyle.width;
            handle.CapturePointer(evt.pointerId);
        }

        private void OnPointerMove(PointerMoveEvent evt, bool isRight)
        {
            if (!_dragging || _dragTarget == null) return;

            float delta    = evt.position.x - _dragStartX;
            float newWidth  = isRight ? _dragStartWidth - delta : _dragStartWidth + delta;

            float otherWidth = isRight ? _settings.resolvedStyle.width : _outliner.resolvedStyle.width;
            float min        = isRight ? OutlinerMin : SettingsMin;
            float max        = _root.resolvedStyle.width - otherWidth - MapMin - HandleWidth * 2f;
            if (max < min) max = min;

            _dragTarget.style.width = Mathf.Clamp(newWidth, min, max);
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            if (!_dragging) return;
            _dragging = false;
            _dragHandle?.ReleasePointer(evt.pointerId);
            _dragHandle = null;
            _dragTarget = null;
            if (_resizeCursor != null)
                UnityEngine.Cursor.SetCursor(null, Vector2.zero, CursorMode.ForceSoftware);
        }

        private static Texture2D RotateClockwise90(Texture2D source)
        {
            int w = source.width;
            int h = source.height;
            Color32[] src = source.GetPixels32();
            Color32[] dst = new Color32[src.Length];

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int nx = h - 1 - y;
                    int ny = x;
                    dst[ny * h + nx] = src[y * w + x];
                }
            }

            var rotated = new Texture2D(h, w, TextureFormat.RGBA32, false);
            rotated.SetPixels32(dst);
            rotated.Apply();
            return rotated;
        }

        private void OnDestroy()
        {
            if (_ownsCursor && _resizeCursor != null)
                Destroy(_resizeCursor);
        }
    }
}
