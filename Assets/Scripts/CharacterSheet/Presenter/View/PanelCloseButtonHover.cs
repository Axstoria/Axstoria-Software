using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CharacterSheet.Presenter.View
{
    public class PanelCloseButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Graphic background;
        [SerializeField] private Graphic label;

        private static readonly Color RestLabelColor  = new Color32(180, 180, 180, 255);
        private static readonly Color HoverLabelColor = new Color32(235, 120, 120, 255);
        private static readonly Color RestBgColor     = new Color(1f, 1f, 1f, 0f);
        private static readonly Color HoverBgColor    = new Color32(82, 82, 82, 255);

        private void OnEnable()
        {
            Apply(false);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            Apply(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Apply(false);
        }

        private void Apply(bool hovered)
        {
            if (background != null) background.color = hovered ? HoverBgColor : RestBgColor;
            if (label != null) label.color = hovered ? HoverLabelColor : RestLabelColor;
        }
    }
}
