using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CharacterSheet.Presenter.View
{
    public class SliderHandleEffects : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        private static readonly Color NormalColor = new Color32(232, 232, 232, 255);
        private static readonly Color HoverColor = new Color32(255, 255, 250, 255);
        private static readonly Color DragColor = new Color32(100, 140, 210, 255);

        [SerializeField] private Graphic handle;

        private bool _hovered;
        private bool _dragged;

        public void OnPointerEnter(PointerEventData eventData)
        {
            _hovered = true;
            Apply();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _hovered = false;
            Apply();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _dragged = true;
            Apply();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _dragged = false;
            Apply();
        }

        private void Apply()
        {
            if (handle == null) return;
            handle.color = _dragged ? DragColor : _hovered ? HoverColor : NormalColor;
        }
    }
}
