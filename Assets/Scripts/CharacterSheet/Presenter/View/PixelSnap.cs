using UnityEngine;

namespace CharacterSheet.Presenter.View
{
    public static class PixelSnap
    {
        public static void SnapRect(RectTransform rt)
        {
            Vector3 scale = rt.lossyScale;
            if (scale.x == 0f || scale.y == 0f) return;

            Vector2 size = rt.rect.size;
            Vector2 snappedSize = new Vector2(
                Mathf.Round(size.x * scale.x) / scale.x,
                Mathf.Round(size.y * scale.y) / scale.y);
            rt.sizeDelta += snappedSize - size;

            Vector3 worldMin = rt.TransformPoint(rt.rect.min);
            Vector2 snappedMin = new Vector2(Mathf.Round(worldMin.x), Mathf.Round(worldMin.y));
            rt.position += (Vector3)(snappedMin - (Vector2)worldMin);
        }

        public static float SnapLength(float value, float scale)
        {
            return scale != 0f ? Mathf.Round(value * scale) / scale : value;
        }
    }
}
