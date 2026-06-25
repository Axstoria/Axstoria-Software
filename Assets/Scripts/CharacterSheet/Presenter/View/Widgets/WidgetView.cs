using System.Collections.Specialized;
using CharacterSheet.Presenter.ViewModel;
using Loxodon.Framework.Binding;
using Loxodon.Framework.ViewModels;
using Loxodon.Framework.Views;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CharacterSheet.Presenter.View
{
    public abstract class WidgetView : UIView, IPointerClickHandler
    {
        [SerializeField] private Image background;
        [SerializeField] private Image border;
        [SerializeField] private RectTransform backgroundOffset;
        [SerializeField] private GameObject selectionBorder;
        
        [SerializeField] private Transform content;
        [SerializeField] private GameObject statContainerPrefab;
        
        public TMP_InputField title;

        private WidgetViewModel _vm;
        private RectTransform _rectTransform;

        public float BorderThickness
        {
            get
            {
                if (backgroundOffset == null) return 0f;
                return backgroundOffset.offsetMin.x;
            }
            set
            {
                if (backgroundOffset == null) return;
                int t = Mathf.RoundToInt(value);
                backgroundOffset.offsetMin = new Vector2(t, t);
                backgroundOffset.offsetMax = new Vector2(-t, -t);
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
            
            bindingSet.Bind(title)
                .For(v => v.text)
                .To(vm => vm.Title)
                .OneWay();

            bindingSet.Bind(title)
                .For(v => v.onEndEdit)
                .To(vm => vm.UpdateTitleCommand);
            
            bindingSet.Bind(selectionBorder).For(v => v.activeSelf).To(vm => vm.IsSelected);

            bindingSet.Build();

            _vm.BoundStats.CollectionChanged += OnStatCollectionChanged;
        }

        private void OnStatCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (StatViewModel newStat in e.NewItems)
                    {
                        CreateStatElement(newStat);
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (StatViewModel oldStat in e.OldItems)
                    {
                        DestroyStatElement(oldStat);
                    }
                    break;

                case NotifyCollectionChangedAction.Reset:
                    ClearAllStats();
                    break;
            }
        }

        private void CreateStatElement(StatViewModel stat)
        {
            GameObject go =  Instantiate(statContainerPrefab, content);
            IStatContainerView view = go.GetComponent<IStatContainerView>();
            
            if (view != null)
                view.SetDataContext(stat);
        }

        private void DestroyStatElement(StatViewModel stat)
        {
            foreach (Transform child in content)
            {
                IStatContainerView statView = child.GetComponent<IStatContainerView>();
                if (statView != null && statView.GetDataContext() == stat)
                {
                    Destroy(child.gameObject);
                    break;
                }
            }
        }

        private void ClearAllStats()
        {
            foreach (Transform child in content)
                Destroy(child.gameObject);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                _vm.Select();
            }
        }
    }
    
}