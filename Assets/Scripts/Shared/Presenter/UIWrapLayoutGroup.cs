using UnityEngine;
using UnityEngine.UI;

namespace Shared.Presenter
{
    [AddComponentMenu("Layout/UI Wrap Layout Group")]
    public class UIWrapLayoutGroup : LayoutGroup
    {
        [SerializeField] private Vector2 cellSize = new Vector2(20, 20);
        [SerializeField] private Vector2 spacing = Vector2.zero;

        public override void CalculateLayoutInputHorizontal() => base.CalculateLayoutInputHorizontal();
        public override void CalculateLayoutInputVertical() { }
        public override void SetLayoutHorizontal() => Wrap(false);
        public override void SetLayoutVertical() => Wrap(true);

        private void Wrap(bool axisVertical)
        {
            float width = rectTransform.rect.width;
            float currentX = 0;
            float currentY = 0;

            for (int i = 0; i < rectChildren.Count; i++)
            {
                var child = rectChildren[i];
                if (currentX + cellSize.x > width && i > 0)
                {
                    currentX = 0;
                    currentY += cellSize.y + spacing.y;
                }

                if (!axisVertical)
                    SetChildAlongAxis(child, 0, currentX, cellSize.x);
                else
                    SetChildAlongAxis(child, 1, currentY, cellSize.y);

                currentX += cellSize.x + spacing.x;
            }
        }
    }
}