using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CharacterSheet.Presenter.View
{
    public class PanelScrollForwarder : MonoBehaviour, IScrollHandler
    {
        [SerializeField] private ScrollRect target;

        public void OnScroll(PointerEventData eventData)
        {
            if (target != null) target.OnScroll(eventData);
        }
    }
}
