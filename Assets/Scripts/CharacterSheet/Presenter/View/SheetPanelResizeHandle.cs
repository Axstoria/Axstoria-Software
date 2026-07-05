using UnityEngine;
using UnityEngine.EventSystems;

namespace CharacterSheet.Presenter.View
{
    public class SheetPanelResizeHandle : MonoBehaviour, IBeginDragHandler, IDragHandler
    {
        [SerializeField] private SheetPanelsController controller;
        [SerializeField] private RectTransform targetPanel;
        [SerializeField] private bool isRight;

        private float _startWidth;
        private Vector2 _startPointerLocal;

        public void OnBeginDrag(PointerEventData eventData)
        {
            eventData.Use();
            var parent = targetPanel != null ? targetPanel.parent as RectTransform : null;
            if (parent == null) return;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parent, eventData.position, eventData.pressEventCamera, out _startPointerLocal);
            _startWidth = targetPanel.sizeDelta.x;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (controller == null || targetPanel == null) return;

            var parent = targetPanel.parent as RectTransform;
            if (parent == null) return;

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    parent, eventData.position, eventData.pressEventCamera, out Vector2 pointerLocal))
                return;

            float delta = pointerLocal.x - _startPointerLocal.x;
            float newWidth = isRight ? _startWidth - delta : _startWidth + delta;
            newWidth = Mathf.Clamp(newWidth, controller.PanelMinWidth, controller.GetMaxWidth(targetPanel));

            controller.SetPanelWidth(targetPanel, newWidth);
        }
    }
}
