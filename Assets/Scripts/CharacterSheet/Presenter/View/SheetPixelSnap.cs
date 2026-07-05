using UnityEngine;

namespace CharacterSheet.Presenter.View
{
    public class SheetPixelSnap : MonoBehaviour
    {
        private RectTransform _rt;
        private Vector2 _lastMin;

        private void Awake()
        {
            _rt = (RectTransform)transform;
        }

        private void LateUpdate()
        {
            Vector2 worldMin = _rt.TransformPoint(_rt.rect.min);
            if ((worldMin - _lastMin).sqrMagnitude < 0.0001f) return;
            PixelSnap.SnapRect(_rt);
            _lastMin = _rt.TransformPoint(_rt.rect.min);
        }
    }
}
