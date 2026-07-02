using UnityEngine;
using UnityEngine.EventSystems;

namespace CharacterSheet.Presenter.View
{
    public class ResizeCursor : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IEndDragHandler
    {
        [SerializeField] private Texture2D cursor;

        private static readonly Vector2 Hotspot = new Vector2(16f, 16f);

        private bool _hovered;
        private bool _dragging;

        public void OnPointerEnter(PointerEventData eventData)
        {
            _hovered = true;
            if (DragCursor.DragActive) return;
            if (cursor != null)
                Cursor.SetCursor(cursor, Hotspot, CursorMode.ForceSoftware);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _hovered = false;
            if (DragCursor.DragActive) return;
            if (!_dragging) ResetCursor();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _dragging = true;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _dragging = false;
            if (!_hovered) ResetCursor();
        }

        private void OnDisable()
        {
            if (!_hovered && !_dragging) return;
            _hovered  = false;
            _dragging = false;
            if (!DragCursor.DragActive) ResetCursor();
        }

        private static void ResetCursor()
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.ForceSoftware);
        }
    }
}
