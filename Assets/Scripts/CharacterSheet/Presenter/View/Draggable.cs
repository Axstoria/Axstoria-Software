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

        public void Awake()
        {
            _canvas = GetComponentInParent<Canvas>();
            _rectTransform = GetComponent<RectTransform>();
            _view = GetComponent<WidgetView>();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
        }

        public void OnDrag(PointerEventData eventData)
        {
            _rectTransform.anchoredPosition += eventData.delta / _canvas.scaleFactor;
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