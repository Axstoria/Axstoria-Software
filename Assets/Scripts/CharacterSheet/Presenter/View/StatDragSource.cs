using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CharacterSheet.Presenter.View
{
    public class StatDragSource : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private static readonly Color GhostBgColor = new Color32(38, 38, 38, 240);
        private static readonly Color GhostTextColor = new Color32(235, 235, 235, 255);
        private static readonly Vector2 GhostOffset = new Vector2(18f, -16f);

        [SerializeField] private Sprite ghostBackground;

        public static StatDragSource Active { get; private set; }

        public string StatId { get; set; }
        public string DisplayName { get; set; }

        private GameObject _ghost;

        public void OnBeginDrag(PointerEventData eventData)
        {
            Active = this;

            var canvas = GetComponentInParent<Canvas>().rootCanvas;

            _ghost = new GameObject("StatDragGhost");
            _ghost.transform.SetParent(canvas.transform, false);
            var background = _ghost.AddComponent<Image>();
            background.sprite = ghostBackground;
            background.type = Image.Type.Sliced;
            background.color = GhostBgColor;
            background.raycastTarget = false;

            var labelGo = new GameObject("Label");
            labelGo.transform.SetParent(_ghost.transform, false);
            var label = labelGo.AddComponent<TextMeshProUGUI>();
            label.text = string.IsNullOrEmpty(DisplayName) ? StatId : DisplayName;
            label.fontSize = 12;
            label.color = GhostTextColor;
            label.alignment = TextAlignmentOptions.Center;
            label.raycastTarget = false;

            var labelRt = (RectTransform)labelGo.transform;
            labelRt.anchorMin = Vector2.zero;
            labelRt.anchorMax = Vector2.one;
            labelRt.sizeDelta = Vector2.zero;

            var size = label.GetPreferredValues(label.text);
            ((RectTransform)_ghost.transform).sizeDelta = new Vector2(size.x + 16f, 22f);

            MoveGhost(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            MoveGhost(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (_ghost != null) Destroy(_ghost);
            Active = null;
        }

        private void MoveGhost(PointerEventData eventData)
        {
            if (_ghost != null) _ghost.transform.position = eventData.position + GhostOffset;
        }

        private void OnDisable()
        {
            if (_ghost != null) Destroy(_ghost);
            if (Active == this) Active = null;
        }
    }
}
