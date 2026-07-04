using System.Collections.Specialized;
using System.Linq;
using CharacterSheet.App.UseCase;
using CharacterSheet.Domain.Widgets;
using Loxodon.Framework.Observables;

namespace CharacterSheet.Presenter.ViewModel.Widgets
{
    public class PointGaugeViewModel : WidgetViewModel
    {
        private readonly PointGaugeWidget _gauge;

        public PointGaugeViewModel(PointGaugeWidget widget,
            BindStatToWidgetUseCase bindStatToWidgetUseCase,
            UnbindStatUseCase unbindStatUseCase,
            UpdateAppearanceUseCase appearance,
            UpdateBackgroundUseCase  background,
            UpdateWidgetLayoutUseCase updateLayout, 
            UpdateWidgetTitleUseCase updateTitle,
            GetStatUseCase getStat,
            UpdatePointGaugeWidgetUseCase up) : base(widget, bindStatToWidgetUseCase, unbindStatUseCase, appearance, background, updateLayout, updateTitle, getStat)
        {
            _gauge = widget;
            foreach (var stat in BoundStats)
                Items.Add(new PointItemViewModel(stat));
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
                        Items.Add(new PointItemViewModel(newStat));
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
        
        public class PointItemViewModel : WidgetItemViewModel
        {
            public ObservableList<int> PointStates { get; } = new();
            public PointItemViewModel(BoundStatViewModel boundStat) : base(boundStat)
            {
                UpdatePoints();
                
                BaseStat.PropertyChanged += (s, e) => UpdatePoints();
            }
            
            private void UpdatePoints()
            {
                PointStates.Clear();
                for (var i = 0; i < BaseStat.MaxValue; i++) {
                    PointStates.Add(i < BaseStat.CurrentValue ? 1 : 0);
                }
            }
        }
    }
}