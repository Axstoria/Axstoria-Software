using System.Collections.Specialized;
using System.Linq;
using CharacterSheet.App.UseCase;
using CharacterSheet.Domain.Widgets;
using UnityEngine;

namespace CharacterSheet.Presenter.ViewModel.Widgets
{
    public class BarWidgetViewModel : WidgetViewModel
    {
        private readonly BarWidget _bar;

        public BarWidgetViewModel(BarWidget widget,
            BindStatToWidgetUseCase bindStatToWidgetUseCase,
            UpdateAppearanceUseCase appearance,
            UpdateBackgroundUseCase  background,
            UpdateWidgetLayoutUseCase updateLayout, 
            UpdateWidgetTitleUseCase updateTitle,
            GetStatUseCase getStat) 
            : base(widget, bindStatToWidgetUseCase, appearance, background, updateLayout, updateTitle, getStat)
        {
            _bar = widget;
            BoundStats.CollectionChanged += OnBoundStatsChanged;
        }

        protected override void HandleContentChanged()
        {
            
        }
        
        private void OnBoundStatsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action) {
                case NotifyCollectionChangedAction.Add:
                    foreach (BoundStatViewModel newStat in e.NewItems) {
                        Items.Add(new ProgressionBarViewModel(newStat));
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
        
        public class ProgressionBarViewModel : WidgetItemViewModel
        {
            public float Fill { get; private set; }
            public ProgressionBarViewModel(BoundStatViewModel boundStat) : base(boundStat)
            {
                UpdateFillAmount();
                BaseStat.PropertyChanged += (s, e) => UpdateFillAmount();
            }
            
            private void UpdateFillAmount()
            {
                float calculatedFill = (float)BaseStat.CurrentValue / (float)BaseStat.MaxValue;
                Fill = Mathf.Clamp01(calculatedFill);
                RaisePropertyChanged(nameof(Fill));
            }
        }
    }
}