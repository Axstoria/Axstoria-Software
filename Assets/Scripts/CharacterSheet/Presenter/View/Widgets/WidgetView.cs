using System.Collections.Specialized;
using CharacterSheet.Presenter.ViewModel;
using Loxodon.Framework.Binding;
using Loxodon.Framework.Views;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CharacterSheet.Presenter.View.Widgets
{
    public abstract class WidgetView : UIView, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Image background;
        [SerializeField] private Image border;
        [SerializeField] private RectTransform backgroundOffset;
        [SerializeField] private GameObject selectionBorder;
        [SerializeField] private GameObject hoverBorder;
        [SerializeField] private GameObject handles;

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
                Vector3 scale = backgroundOffset.lossyScale;
                float tx = PixelSnap.SnapLength(Mathf.RoundToInt(value), scale.x);
                float ty = PixelSnap.SnapLength(Mathf.RoundToInt(value), scale.y);
                backgroundOffset.offsetMin = new Vector2(tx, ty);
                backgroundOffset.offsetMax = new Vector2(-tx, -ty);
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
                PixelSnap.SnapRect(RectTransform);
            }
        }

        protected override void Start()
        {
            base.Start();

            _vm = this.BindingContext().DataContext as WidgetViewModel;
            if (_vm == null) return;

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

            if (handles != null)
                bindingSet.Bind(handles).For(v => v.activeSelf).To(vm => vm.IsSelected);

            bindingSet.Build();

            foreach (var stat in _vm.Items)
                OnStatAdded(stat);

            _vm.Items.CollectionChanged += OnStatCollectionChanged;
        }

        private void OnStatCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action) {
                case NotifyCollectionChangedAction.Add:
                    foreach (WidgetItemViewModel newStat in e.NewItems) {
                        OnStatAdded(newStat);
                    }

                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (WidgetItemViewModel oldStat in e.OldItems) {
                        OnStatRemoved(oldStat);
                    }

                    break;

                case NotifyCollectionChangedAction.Reset:
                    ClearAllStats();
                    break;
            }
        }

        protected virtual void OnStatAdded(WidgetItemViewModel boundStat)
        {
            GameObject go = Instantiate(statContainerPrefab, content);
            IStatContainerView view = go.GetComponent<IStatContainerView>();

            if (view != null)
                view.SetDataContext(boundStat);
        }

        protected virtual void OnStatRemoved(WidgetItemViewModel boundStat)
        {
            foreach (Transform child in content) {
                IStatContainerView statView = child.GetComponent<IStatContainerView>();
                if (statView != null && statView.GetDataContext() == boundStat) {
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
            if (eventData.button == PointerEventData.InputButton.Left) {
                _vm.Select();
                if (hoverBorder != null) hoverBorder.SetActive(false);
            }
            else if (eventData.button == PointerEventData.InputButton.Right) {
                var sheetView = GetComponentInParent<SheetView>();
                if (sheetView != null) sheetView.ClearSelection();
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (hoverBorder != null && _vm != null && !_vm.IsSelected)
                hoverBorder.SetActive(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (hoverBorder != null)
                hoverBorder.SetActive(false);
        }
    }
}