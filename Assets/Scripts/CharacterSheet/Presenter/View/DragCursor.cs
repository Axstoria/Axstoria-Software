using UnityEngine;
using UnityEngine.EventSystems;

namespace CharacterSheet.Presenter.View
{
    public class DragCursor : MonoBehaviour, IBeginDragHandler, IEndDragHandler
    {
        [SerializeField] private Texture2D cursor;

        private static readonly Vector2 Hotspot = new Vector2(16f, 16f);

        public static bool DragActive { get; private set; }

        public void OnBeginDrag(PointerEventData eventData)
        {
            DragActive = true;
            if (cursor != null)
                Cursor.SetCursor(cursor, Hotspot, CursorMode.ForceSoftware);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            DragActive = false;
            ResetCursor();
        }

        private void OnDisable()
        {
            if (!DragActive) return;
            DragActive = false;
            ResetCursor();
        }

        private static void ResetCursor()
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.ForceSoftware);
        }
    }
}
