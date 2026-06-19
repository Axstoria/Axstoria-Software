using System;
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
        [SerializeField] private Vector2 direction;

        [Header("Widget's limit")]
        [SerializeField] private float minWidth = 100f;
        [SerializeField] private float minHeight = 60f;

        private Canvas _canvas;
        private WidgetView _view;
        private void Awake()
        {
            _canvas = GetComponentInParent<Canvas>();
            _view = GetComponentInParent<WidgetView>();
        }
        
        public void OnBeginDrag(PointerEventData eventData)
        {
            eventData.Use(); 
        }
        
        public void OnDrag(PointerEventData eventData)
        {
            if (_canvas == null || targetRectTransform == null) return;
            
            Vector2 mouseDelta = eventData.delta /  _canvas.scaleFactor;
            
            Vector2 sizeDelta = targetRectTransform.sizeDelta;
            Vector2 currentPos = targetRectTransform.anchoredPosition;
            
            float deltaWidth = mouseDelta.x * direction.x;
            float deltaHeight = mouseDelta.y * direction.y;
            
            float newWidth = Mathf.Max(minWidth, sizeDelta.x + deltaWidth);
            float newHeight = Mathf.Max(minHeight, sizeDelta.y + deltaHeight);
            
            float actualDeltaWidth = newWidth - sizeDelta.x;
            float actualDeltaHeight = newHeight - sizeDelta.y;
            
            targetRectTransform.sizeDelta = new Vector2(newWidth, newHeight);

            float posXOffset = (direction.x < 0) ? -1 * actualDeltaWidth : 0;
            float posYOffset = (direction.y > 0) ? actualDeltaHeight : 0;

            targetRectTransform.anchoredPosition = new Vector2(
                currentPos.x + posXOffset,
                currentPos.y + posYOffset
            );
            
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (_view == null) return;
            
            var viewModel = _view.BindingContext().DataContext as WidgetViewModel;
    
            if (viewModel != null)
            {
                Rect finalRect = new Rect(
                    targetRectTransform.anchoredPosition.x,
                    targetRectTransform.anchoredPosition.y,
                    targetRectTransform.sizeDelta.x,
                    targetRectTransform.sizeDelta.y
                );
                
                if (viewModel.UpdateLayoutCommand.CanExecute(finalRect))
                {
                    viewModel.UpdateLayoutCommand.Execute(finalRect);
                }
            }
        }
    }
}
