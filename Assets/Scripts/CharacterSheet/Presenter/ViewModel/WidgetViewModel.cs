using System;
using System.Linq;
using CharacterSheet.App.DTO;
using CharacterSheet.App.UseCase;
using CharacterSheet.Domain;
using Loxodon.Framework.Commands;
using Loxodon.Framework.Observables;
using UnityEngine;

namespace CharacterSheet.Presenter.ViewModel
{
    public abstract class WidgetItemViewModel : ObservableObject
    {
        public string Id => BaseStat.Id;
        public BoundStatViewModel BaseStat { get; }
        protected WidgetItemViewModel(BoundStatViewModel stat) { BaseStat = stat; }
    }
    
    public abstract class WidgetViewModel : ObservableObject
    {
        private readonly SheetWidget _widget;

        public string Id => _widget.Id;

        public ObservableList<BoundStatViewModel> BoundStats { get; } = new();
        public ObservableList<WidgetItemViewModel> Items { get; } = new ObservableList<WidgetItemViewModel>();  

        // ── Use Case ───────────────────────────────────────────────────────────
        private readonly UpdateAppearanceUseCase _updateAppearance;
        private readonly UpdateBackgroundUseCase _updateBackground;
        private readonly UpdateWidgetLayoutUseCase _updateLayout;
        private readonly UpdateWidgetTitleUseCase _updateTitle;
        private readonly BindStatToWidgetUseCase _bindStat;
        private readonly UnbindStatUseCase _unbindStat;
        private readonly GetStatUseCase _getStat;

        // ── Command ───────────────────────────────────────────────────────────
        public ICommand<Rect> UpdateLayoutCommand { get; }
        public ICommand<string> UpdateTitleCommand { get; }
        public ICommand<AppearanceDTO> UpdateAppearanceCommand { get; }
        public ICommand<string> AddStatCommand { get; }
        public ICommand<string> RemoveStatCommand { get; }
        public event Action<WidgetViewModel> OnSelected;
        public event Action OnWidgetChanged;

        public void Select() => OnSelected?.Invoke(this);

        private bool _isSelected;

        public bool IsSelected
        {
            get => _isSelected;
            set => Set(ref _isSelected, value);
        }

        public Rect Layout => _widget.Layout;
        public bool HasBorder => _widget.HasBorder;
        public float BorderThickness => _widget.BorderThickness;
        public Color BorderColor => _widget.BorderColor;
        public Color BackgroundColor => _widget.BackgroundColor;
        public string BackgroundImagePath => _widget.BackgroundImagePath;
        public string Title => _widget.Title;

        protected WidgetViewModel(SheetWidget widget,
            BindStatToWidgetUseCase bindStatToWidgetUseCase,
            UnbindStatUseCase unbindStatUseCase,
            UpdateAppearanceUseCase updateAppearance,
            UpdateBackgroundUseCase updateBackground,
            UpdateWidgetLayoutUseCase updateLayout,
            UpdateWidgetTitleUseCase updateTitle,
            GetStatUseCase getStat)
        {
            _widget = widget;

            _updateAppearance = updateAppearance;
            _updateBackground = updateBackground;
            _updateLayout = updateLayout;
            _updateTitle = updateTitle;
            _bindStat = bindStatToWidgetUseCase;
            _unbindStat =  unbindStatUseCase;
            _getStat = getStat;

            foreach (var binding in widget.Stats)
                LoadStat(binding);

            _widget.OnLayoutChanged += HandleLayoutChanged;
            UpdateLayoutCommand = new SimpleCommand<Rect>(rec => { _updateLayout.Execute(_widget, rec); });

            _widget.OnAppearanceChanged += HandleAppearanceChanged;
            UpdateAppearanceCommand = new SimpleCommand<AppearanceDTO>(appearance =>
            {
                _updateAppearance.Execute(_widget, appearance);
            });

            _widget.OnTitleChanged += HandleTitleChanged;
            UpdateTitleCommand = new SimpleCommand<string>(text => { _updateTitle.Execute(_widget, text); });

            AddStatCommand = new SimpleCommand<string>(id => {_bindStat.Execute(_widget, id); });
            RemoveStatCommand = new SimpleCommand<string>(id => {_unbindStat.Execute(_widget, id); });

            _widget.OnStatAdded += HandleStatAdded;
            _widget.OnStatRemoved += HandleStatRemoved;
            _widget.OnContentChanged += HandleContentChanged;
        }

        private void LoadStat(WidgetStatBinding stat)
        {
            BoundStats.Add(new BoundStatViewModel(stat, _getStat.Execute(stat.StatId)));
        }

        private void HandleLayoutChanged()
        {
            RaisePropertyChanged(nameof(Layout));
            OnWidgetChanged?.Invoke();
        }

        private void HandleAppearanceChanged()
        {
            RaisePropertyChanged(nameof(HasBorder));
            RaisePropertyChanged(nameof(BorderThickness));
            RaisePropertyChanged(nameof(BorderColor));
            RaisePropertyChanged(nameof(BackgroundColor));
            RaisePropertyChanged(nameof(BackgroundImagePath));
            OnWidgetChanged?.Invoke();
        }

        private void HandleTitleChanged()
        {
            RaisePropertyChanged(nameof(Title));
            OnWidgetChanged?.Invoke();
        }

        private void HandleStatAdded(WidgetStatBinding stat)
        {
            LoadStat(stat);
            OnWidgetChanged?.Invoke();
        }

        private void HandleStatRemoved(WidgetStatBinding stat)
        {
            var boundStat = BoundStats.FirstOrDefault(s => s.Id == stat.StatId);
            if (boundStat != null) {
                BoundStats.Remove(boundStat);
                boundStat.Dispose();
            }
            OnWidgetChanged?.Invoke();
        }

        protected abstract void HandleContentChanged();

        public void Dispose()
        {
            if (_widget != null) {
                _widget.OnLayoutChanged -= HandleLayoutChanged;
                _widget.OnAppearanceChanged -= HandleAppearanceChanged;
                _widget.OnTitleChanged -= HandleTitleChanged;
                _widget.OnStatAdded -= HandleStatAdded;
                _widget.OnStatRemoved -= HandleStatRemoved;
                _widget.OnContentChanged -= HandleContentChanged;
            }
        }
    }
}