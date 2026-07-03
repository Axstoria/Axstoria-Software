using System.Collections.Generic;
using System.Collections.Specialized;
using CharacterSheet.Presenter.ViewModel;
using CharacterSheet.Presenter.ViewModel.Widgets;
using Loxodon.Framework.Binding;
using UnityEngine;
using UnityEngine.UI;

namespace CharacterSheet.Presenter.View.Widgets.PointGaugeWidget
{
    public class PointsContainerView : IStatContainerView
    {
        private static readonly Color FilledColor = new Color32(50, 50, 50, 255);
        private static readonly Color EmptyColor = new Color32(225, 225, 225, 255);

        [SerializeField] private GameObject iconPrefab;
        [SerializeField] private Sprite fullIconSprite;
        [SerializeField] private Sprite emptyIconSprite;
        private List<Image> _icons = new List<Image>();
        private PointGaugeViewModel.PointItemViewModel _itemViewModel;

        protected override void Start()
        {
            base.Start();

            _itemViewModel = this.BindingContext().DataContext as PointGaugeViewModel.PointItemViewModel;
            _vm = _itemViewModel;
            if (_itemViewModel == null) return;
            
            for (int i = 0; i < _itemViewModel.BaseStat.MaxValue; i++)
            {
                var go = Instantiate(iconPrefab, transform);
                _icons.Add(go.GetComponent<Image>());
            }
            
            _itemViewModel.PointStates.CollectionChanged += RefreshIcons;
            RefreshIcons();
        }

        private void RefreshIcons(object sender = null, NotifyCollectionChangedEventArgs e = null)
        {
            for (int i = 0; i < _icons.Count; i++)
            {
                int state = _itemViewModel.PointStates[i];
                
                if (state == 1) {
                    _icons[i].sprite = fullIconSprite;
                    _icons[i].color = FilledColor;
                } else {
                    _icons[i].sprite = fullIconSprite;
                    _icons[i].color = EmptyColor;
                }
            }
        }
    }
}