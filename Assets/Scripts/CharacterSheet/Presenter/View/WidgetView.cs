using CharacterSheet.Presenter.ViewModel;
using Loxodon.Framework.Binding;
using Loxodon.Framework.ViewModels;
using Loxodon.Framework.Views;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CharacterSheet.Presenter.View
{
    public abstract class WidgetView : UIView, IPointerClickHandler
    {
        [SerializeField] private Image background;
        [SerializeField] private Image border;
        [SerializeField] private HorizontalLayoutGroup borderLayoutGroup;
        [SerializeField] private GameObject selectionBorder;

        private WidgetViewModel _vm;
        private RectTransform _rectTransform;

        public float BorderThickness
        {
            get
            {
                if (borderLayoutGroup == null) return 0f;
                return borderLayoutGroup.padding.left;
            }
            set
            {
                if (borderLayoutGroup == null) return;
                int t = Mathf.RoundToInt(value);
                borderLayoutGroup.padding.left = t;
                borderLayoutGroup.padding.right = t;
                borderLayoutGroup.padding.top = t;
                borderLayoutGroup.padding.bottom = t;
                
                borderLayoutGroup.SetLayoutHorizontal();
                borderLayoutGroup.SetLayoutVertical();
            }
        }
        
        public Rect WidgetLayout
        {
            get => new Rect(RectTransform.anchoredPosition, RectTransform.sizeDelta);
            set
            {
                if (RectTransform == null) return;
                RectTransform.anchoredPosition = value.position;
                RectTransform.sizeDelta = value.size;
            }
        }
        
        //private RectTransform RectTransform => _rectTransform ??= GetComponent<RectTransform>();

        protected override void Start()
        {
            base.Start();
            
            _vm = this.BindingContext().DataContext as WidgetViewModel;
            if(_vm == null) return;
            
            var bindingSet = this.CreateBindingSet<WidgetView, WidgetViewModel>();
            
            bindingSet.Bind(background).For(v => v.color)
                .To(vm => vm.BackgroundColor);
            bindingSet.Bind(border).For(v => v.enabled)
                .To(vm => vm.HasBorder);
            
            bindingSet.Bind(border)
                .For(v => v.color)
                .To(vm => vm.BorderColor);
            
            bindingSet.Bind(this)
                .For(v => v.BorderThickness)
                .To(vm => vm.BorderThickness);
            
            bindingSet.Bind(this)
                .For(v => v.WidgetLayout)
                .To(vm => vm.Layout);
                
            
            bindingSet.Bind(selectionBorder).For(v => v.activeSelf).To(vm => vm.IsSelected);

            bindingSet.Build();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Debug.Log("OnPointerClick");
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                _vm.Select();
            }
        }
    }
}