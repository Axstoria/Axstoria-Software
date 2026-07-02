using System;
using CharacterSheet.Presenter.View.Widgets;
using CharacterSheet.Presenter.ViewModel;
using Loxodon.Framework.Binding;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CharacterSheet.Presenter.View
{
    public class Draggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private Canvas _canvas;
        private RectTransform _rectTransform;
        private WidgetView _view;
        private RectTransform _bounds;
        private readonly Vector3[] _boundsCorners = new Vector3[4];

        public void Awake()
        {
            _canvas = GetComponentInParent<Canvas>();
            _rectTransform = GetComponent<RectTransform>();
            _view = GetComponent<WidgetView>();

            var sheet = GetComponentInParent<SheetView>();
            _bounds = sheet != null ? sheet.WidgetArea : null;
        }

        private Vector2 _startPointerLocal;
        private Vector2 _startPos;

        public void OnBeginDrag(PointerEventData eventData)
        {
            var parent = _rectTransform.parent as RectTransform;
            if (parent == null) return;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parent, eventData.position, eventData.pressEventCamera, out _startPointerLocal);
            _startPos = _rectTransform.anchoredPosition;
        }

        public void OnDrag(PointerEventData eventData)
        {
            var parent = _rectTransform.parent as RectTransform;
            if (parent == null) return;

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    parent, eventData.position, eventData.pressEventCamera, out Vector2 pointerLocal))
                return;

            Vector2 desired = _startPos + (pointerLocal - _startPointerLocal);
            _rectTransform.anchoredPosition = ClampToParent(desired);
        }

        private Vector2 ClampToParent(Vector2 anchoredPos)
        {
            var parent = _rectTransform.parent as RectTransform;
            if (parent == null) return anchoredPos;

            Rect parentRect = parent.rect;
            Rect limits = GetBoundsRect(parent, parentRect);
            Vector2 size = _rectTransform.rect.size;
            Vector2 anchor = new Vector2(
                parentRect.xMin + parentRect.width  * (_rectTransform.anchorMin.x + _rectTransform.anchorMax.x) * 0.5f,
                parentRect.yMin + parentRect.height * (_rectTransform.anchorMin.y + _rectTransform.anchorMax.y) * 0.5f);
            Vector2 min = anchor + anchoredPos - Vector2.Scale(_rectTransform.pivot, size);

            Vector2 offset = Vector2.zero;
            if (min.x < limits.xMin)                offset.x = limits.xMin - min.x;
            else if (min.x + size.x > limits.xMax)  offset.x = limits.xMax - (min.x + size.x);
            if (min.y < limits.yMin)                offset.y = limits.yMin - min.y;
            else if (min.y + size.y > limits.yMax)  offset.y = limits.yMax - (min.y + size.y);

            return anchoredPos + offset;
        }

        private Rect GetBoundsRect(RectTransform parent, Rect fallback)
        {
            if (_bounds == null) return fallback;
            _bounds.GetWorldCorners(_boundsCorners);
            Vector2 min = parent.InverseTransformPoint(_boundsCorners[0]);
            Vector2 max = parent.InverseTransformPoint(_boundsCorners[2]);
            return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (_view == null) return;
            var viewModel = _view.BindingContext().DataContext as WidgetViewModel;

            if (viewModel != null) {
                Rect finalRect = new Rect(
                    _rectTransform.anchoredPosition.x,
                    _rectTransform.anchoredPosition.y,
                    _rectTransform.sizeDelta.x,
                    _rectTransform.sizeDelta.y
                );

                if (viewModel.UpdateLayoutCommand.CanExecute(finalRect)) {
                    viewModel.UpdateLayoutCommand.Execute(finalRect);
                }
            }
        }
    }
}