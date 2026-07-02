using System;
using CharacterSheet.Presenter.View.Widgets;
using CharacterSheet.Presenter.ViewModel;
using Loxodon.Framework.Binding;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CharacterSheet.Presenter.View
{
    public class ResizeHandle : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private RectTransform targetRectTransform;

        [Header("Directions (choose 1, 0 or -1)")]
        [Tooltip("1 = right/top, -1 = Bottom/Left, 0 = nothing")]
        [SerializeField]
        private Vector2 direction;

        [Header("Widget's limit")] [SerializeField]
        private float minWidth = 100f;

        [SerializeField] private float minHeight = 60f;

        private Canvas _canvas;
        private WidgetView _view;
        private RectTransform _bounds;
        private readonly Vector3[] _boundsCorners = new Vector3[4];

        private Vector2 _startPointerLocal;
        private Vector2 _startSize;
        private Vector2 _startPos;

        private void Awake()
        {
            _canvas = GetComponentInParent<Canvas>();
            _view = GetComponentInParent<WidgetView>();

            var sheet = GetComponentInParent<SheetView>();
            _bounds = sheet != null ? sheet.WidgetArea : null;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            eventData.Use();

            var parent = targetRectTransform != null ? targetRectTransform.parent as RectTransform : null;
            if (parent == null) return;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parent, eventData.position, eventData.pressEventCamera, out _startPointerLocal);
            _startSize = targetRectTransform.sizeDelta;
            _startPos = targetRectTransform.anchoredPosition;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_canvas == null || targetRectTransform == null) return;

            var parent = targetRectTransform.parent as RectTransform;
            if (parent == null) return;

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    parent, eventData.position, eventData.pressEventCamera, out Vector2 pointerLocal))
                return;

            Vector2 totalDelta = pointerLocal - _startPointerLocal;

            float deltaWidth = totalDelta.x * direction.x;
            float deltaHeight = totalDelta.y * direction.y;

            Rect parentRect = parent.rect;
            Rect limits = parentRect;
            if (_bounds != null) {
                _bounds.GetWorldCorners(_boundsCorners);
                Vector2 bMin = parent.InverseTransformPoint(_boundsCorners[0]);
                Vector2 bMax = parent.InverseTransformPoint(_boundsCorners[2]);
                limits = Rect.MinMaxRect(bMin.x, bMin.y, bMax.x, bMax.y);
            }

            Vector2 anchor = new Vector2(
                parentRect.xMin + parentRect.width  * (targetRectTransform.anchorMin.x + targetRectTransform.anchorMax.x) * 0.5f,
                parentRect.yMin + parentRect.height * (targetRectTransform.anchorMin.y + targetRectTransform.anchorMax.y) * 0.5f);
            Vector2 min = anchor + _startPos - Vector2.Scale(targetRectTransform.pivot, _startSize);
            Vector2 max = min + _startSize;

            if (direction.x > 0)      deltaWidth  = Mathf.Min(deltaWidth,  limits.xMax - max.x);
            else if (direction.x < 0) deltaWidth  = Mathf.Min(deltaWidth,  min.x - limits.xMin);
            if (direction.y > 0)      deltaHeight = Mathf.Min(deltaHeight, limits.yMax - max.y);
            else if (direction.y < 0) deltaHeight = Mathf.Min(deltaHeight, min.y - limits.yMin);

            float newWidth = Mathf.Max(minWidth, _startSize.x + deltaWidth);
            float newHeight = Mathf.Max(minHeight, _startSize.y + deltaHeight);

            float actualDeltaWidth = newWidth - _startSize.x;
            float actualDeltaHeight = newHeight - _startSize.y;

            targetRectTransform.sizeDelta = new Vector2(newWidth, newHeight);

            float posXOffset = (direction.x < 0) ? -1 * actualDeltaWidth : 0;
            float posYOffset = (direction.y > 0) ? actualDeltaHeight : 0;

            targetRectTransform.anchoredPosition = new Vector2(
                _startPos.x + posXOffset,
                _startPos.y + posYOffset
            );
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (_view == null) return;

            var viewModel = _view.BindingContext().DataContext as WidgetViewModel;

            if (viewModel != null) {
                Rect finalRect = new Rect(
                    targetRectTransform.anchoredPosition.x,
                    targetRectTransform.anchoredPosition.y,
                    targetRectTransform.sizeDelta.x,
                    targetRectTransform.sizeDelta.y
                );

                if (viewModel.UpdateLayoutCommand.CanExecute(finalRect)) {
                    viewModel.UpdateLayoutCommand.Execute(finalRect);
                }
            }
        }
    }
}