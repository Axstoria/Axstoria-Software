using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CharacterSheet.Presenter.View
{
    public class StatDropTarget : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
    {
        private static readonly Color RestColor = new Color(1f, 1f, 1f, 0f);
        private static readonly Color HighlightColor = new Color(1f, 1f, 1f, 0.05f);

        [SerializeField] private Graphic highlight;
        [SerializeField] private StatsBlockView statsBlock;

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (StatDragSource.Active != null && highlight != null)
                highlight.color = HighlightColor;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (highlight != null) highlight.color = RestColor;
        }

        public void OnDrop(PointerEventData eventData)
        {
            var drag = StatDragSource.Active;
            if (drag == null) return;
            if (statsBlock != null) statsBlock.TryAddStat(drag.StatId, drag.DisplayName);
            if (highlight != null) highlight.color = RestColor;
        }
    }
}
