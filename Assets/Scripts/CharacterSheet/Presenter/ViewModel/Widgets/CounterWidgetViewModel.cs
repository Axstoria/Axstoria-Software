using System.Collections.Specialized;
using System.Linq;
using CharacterSheet.App.UseCase;
using CharacterSheet.Domain.Widgets;
using Loxodon.Framework.Observables;
using UnityEngine;

namespace CharacterSheet.Presenter.ViewModel.Widgets
{
    public class CounterWidgetViewModel : WidgetViewModel
    {
        private readonly CounterWidget _counter;

        public CounterWidgetViewModel(CounterWidget widget, 
            BindStatToWidgetUseCase bindStatToWidgetUseCase,
            UnbindStatUseCase  unbindStatUseCase,
            UpdateAppearanceUseCase appearance, 
            UpdateBackgroundUseCase background,
            UpdateWidgetLayoutUseCase updateLayout, 
            UpdateWidgetTitleUseCase updateTitle,
            GetStatUseCase getStat) : base(widget, bindStatToWidgetUseCase, unbindStatUseCase, appearance, background, updateLayout, updateTitle, getStat)
        {
            _counter = widget;
            BoundStats.CollectionChanged += OnBoundStatsChanged;
        }

        private void OnBoundStatsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action) {
                case NotifyCollectionChangedAction.Add:
                    foreach (BoundStatViewModel newStat in e.NewItems) {
                        Items.Add(new CounterItemViewModel(newStat));
                    }

                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (BoundStatViewModel oldStat in e.OldItems) {
                        var item = Items.FirstOrDefault(i => i.Id == oldStat.Id);
                        if (item != null) {
                            Items.Remove(item);
                        }
                    }

                    break;

                case NotifyCollectionChangedAction.Reset:
                    Items.Clear();
                    break;
            }
        }

        protected override void HandleContentChanged()
        {
            throw new System.NotImplementedException();
        }
        
        public class CounterItemViewModel : WidgetItemViewModel
        {
            
            public string FormattedText 
            {
                get => BaseStat.MaxValue > 0
                    ? $"<b>{BaseStat.DisplayName}</b> : <color=#555555>{BaseStat.CurrentValue} / {BaseStat.MaxValue}</color>"
                    : $"<b>{BaseStat.DisplayName}</b> : <color=#555555>{BaseStat.CurrentValue}</color>";
            }

            public Color TextColor => BaseStat.Color; 

            public CounterItemViewModel(BoundStatViewModel boundStat) : base(boundStat)
            {
            }
        }
    }
}