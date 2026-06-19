using System.Collections.Generic;
using System.Collections.Specialized;
using CharacterSheet.Domain;
using CharacterSheet.Presenter.View.Widgets;
using CharacterSheet.Presenter.ViewModel;
using CharacterSheet.Presenter.ViewModel.Widgets;
using Loxodon.Framework.Binding;
using Loxodon.Framework.Views;
using UnityEngine;

namespace CharacterSheet.Presenter.View
{
    [System.Serializable]
    public struct WidgetPrefabMapping
    {
        public WidgetType widgetType;
        public GameObject prefab;
    }
    
    public class SheetView : UIView
    {
        [SerializeField] private Transform widgetContainer;
        [SerializeField] private GameObject pointGaugeWidgetPrefab;
        
        private SheetViewModel _vm;
        private readonly Dictionary<string, WidgetView> _widgets = new Dictionary<string, WidgetView>();
        
        /*public float BorderThickness
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
        }*/

        public void Initialize(SheetViewModel vm)
        {
            _vm = vm;
            
            foreach (var widget in vm.Widgets)
                SpawnWidgetView(widget);
            
            vm.Widgets.CollectionChanged += OnWidgetsChanged;
        }

        private void OnWidgetsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
                foreach (WidgetViewModel vm in e.NewItems)
                    SpawnWidgetView(vm);
            if (e.OldItems != null)
                foreach (WidgetViewModel vm in e.OldItems)
                    DestroyWidgetViews(vm.Id);
        }

        private void SpawnWidgetView(WidgetViewModel vm)
        {
            var widgetPrefab = vm switch
            {
                PointGaugeViewModel => pointGaugeWidgetPrefab,
                _ => null
            };
            var go = Instantiate(widgetPrefab, widgetContainer);
            var view = go.GetComponent<WidgetView>();
            view.SetDataContext(vm);
            _widgets.Add(vm.Id, view);
        }

        private void DestroyWidgetViews(string id)
        {
            if (_widgets.TryGetValue(id, out WidgetView view))
            {
                Destroy(view.gameObject);
                _widgets.Remove(id);
            }
        }

        protected override void OnDestroy()
        {
            if (_vm != null && _vm.Widgets != null)
            {
                _vm.Widgets.CollectionChanged -= OnWidgetsChanged;
            }
            base.OnDestroy();
        }
    }
}