using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CharacterSheet.Presenter.View
{
    public class StatRowHighlight : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private static readonly Color RestColor = new Color(1f, 1f, 1f, 0f);
        private static readonly Color HoverColor = new Color(1f, 1f, 1f, 0.06f);

        [SerializeField] private Graphic background;

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (background != null) background.color = HoverColor;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (background != null) background.color = RestColor;
        }
    }
}
