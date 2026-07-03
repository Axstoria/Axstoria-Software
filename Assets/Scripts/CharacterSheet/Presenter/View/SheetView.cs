using System.Collections.Generic;
using System.Collections.Specialized;
using CharacterSheet.Domain;
using CharacterSheet.Presenter.View.Widgets;
using CharacterSheet.Presenter.ViewModel;
using CharacterSheet.Presenter.ViewModel.Widgets;
using Loxodon.Framework.Binding;
using Loxodon.Framework.Views;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CharacterSheet.Presenter.View
{
    [System.Serializable]
    public struct WidgetPrefabMapping
    {
        public WidgetType widgetType;
        public GameObject prefab;
    }

    public class SheetView : UIView, IPointerClickHandler
    {
        [SerializeField] private Image background;
        [SerializeField] private Image border;
        [SerializeField] private HorizontalLayoutGroup borderLayoutGroup;
        
        [Tooltip("Config Button")]
        [SerializeField] private Button importImage;

        [Tooltip("Widgets Prefab")]
        [SerializeField] private Transform widgetContainer;
        [SerializeField] private GameObject pointGaugeWidgetPrefab;
        [SerializeField] private GameObject textWidgetPrefab;
        [SerializeField] private GameObject counterWidgetPrefab;

        private SheetViewModel _vm;
        private readonly Dictionary<string, WidgetView> _widgets = new Dictionary<string, WidgetView>();

        public RectTransform WidgetArea => background != null ? background.rectTransform : null;

        private readonly Vector3[] _areaCorners = new Vector3[4];

        private float _borderThickness;

        public float BorderThickness
        {
            get => _borderThickness;
            set
            {
                _borderThickness = value;
                ApplyBorderPadding();
            }
        }

        public bool HasBorder
        {
            get => border != null && border.enabled;
            set
            {
                if (border != null) border.enabled = value;
                ApplyBorderPadding();
            }
        }

        private void ApplyBorderPadding()
        {
            if (borderLayoutGroup == null) return;
            int t = HasBorder ? Mathf.RoundToInt(_borderThickness) : 0;
            borderLayoutGroup.padding.left = t;
            borderLayoutGroup.padding.right = t;
            borderLayoutGroup.padding.top = t;
            borderLayoutGroup.padding.bottom = t;

            borderLayoutGroup.SetLayoutHorizontal();
            borderLayoutGroup.SetLayoutVertical();
        }

        public void Initialize(SheetViewModel viewModel)
        {
            _vm = viewModel;

            var bindingSet = this.CreateBindingSet<SheetView, SheetViewModel>();

            bindingSet.Bind(background).For(v => v.color)
                .To(vm => vm.BackgroundColor);
            bindingSet.Bind(this).For(v => v.HasBorder)
                .To(vm => vm.HasBorder);

            bindingSet.Bind(border)
                .For(v => v.color)
                .To(vm => vm.BorderColor);

            bindingSet.Bind(this)
                .For(v => v.BorderThickness)
                .To(vm => vm.BorderThickness);
            
            bindingSet.Bind(background)
                .For(v => v.sprite)
                .To(vm => vm.BackgroundSprite);

            /*bindingSet.Bind(importImage).For(v => v.onClick).To(vm => vm.SelectBackgroundCommand);*/

            bindingSet.Build();

            foreach (var widget in _vm.Widgets)
                SpawnWidgetView(widget);

            _vm.Widgets.CollectionChanged += OnWidgetsChanged;
        }

        private void OnWidgetsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
                foreach (WidgetViewModel vm in e.NewItems) {
                    SpawnWidgetView(vm);
                    CenterWidget(vm);
                    vm.Select();
                }
            if (e.OldItems != null)
                foreach (WidgetViewModel vm in e.OldItems)
                    DestroyWidgetViews(vm.Id);
        }

        private void CenterWidget(WidgetViewModel vm)
        {
            var area = WidgetArea;
            var container = widgetContainer as RectTransform;
            if (area == null || container == null) return;
            if (!_widgets.TryGetValue(vm.Id, out WidgetView view)) return;

            var rt = (RectTransform)view.transform;
            area.GetWorldCorners(_areaCorners);
            Vector2 areaMin = container.InverseTransformPoint(_areaCorners[0]);
            Vector2 areaMax = container.InverseTransformPoint(_areaCorners[2]);

            Rect containerRect = container.rect;
            Vector2 anchor = new Vector2(
                containerRect.xMin + containerRect.width  * (rt.anchorMin.x + rt.anchorMax.x) * 0.5f,
                containerRect.yMin + containerRect.height * (rt.anchorMin.y + rt.anchorMax.y) * 0.5f);

            Vector2 size = vm.Layout.size;
            Vector2 minPos = (areaMin + areaMax) * 0.5f - size * 0.5f;
            Vector2 anchoredPos = minPos + Vector2.Scale(rt.pivot, size) - anchor;

            var centered = new Rect(anchoredPos, size);
            if (vm.UpdateLayoutCommand.CanExecute(centered))
                vm.UpdateLayoutCommand.Execute(centered);
        }

        private void SpawnWidgetView(WidgetViewModel vm)
        {
            var widgetPrefab = vm switch
            {
                PointGaugeViewModel => pointGaugeWidgetPrefab,
                TextWidgetViewModel => textWidgetPrefab,
                CounterWidgetViewModel => counterWidgetPrefab,
                _ => null
            };
            var go = Instantiate(widgetPrefab, widgetContainer);
            var view = go.GetComponent<WidgetView>();
            view.SetDataContext(vm);
            _widgets.Add(vm.Id, view);
        }

        private void DestroyWidgetViews(string id)
        {
            if (_widgets.TryGetValue(id, out WidgetView view)) {
                Destroy(view.gameObject);
                _widgets.Remove(id);
            }
        }

        protected override void OnDestroy()
        {
            if (_vm != null && _vm.Widgets != null) {
                _vm.Widgets.CollectionChanged -= OnWidgetsChanged;
            }

            base.OnDestroy();
        }

        public void ClearSelection()
        {
            _vm?.ClearSelection();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            ClearSelection();
        }
    }
}