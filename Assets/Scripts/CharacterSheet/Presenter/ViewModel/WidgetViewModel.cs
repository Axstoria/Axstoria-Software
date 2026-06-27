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
    public abstract class WidgetViewModel : ObservableObject
    {
        private readonly SheetWidget _widget;

        public string Id => _widget.Id;

        public ObservableList<StatViewModel> BoundStats { get; } = new();

        // ── Use Case ───────────────────────────────────────────────────────────
        private readonly UpdateWidgetAppearanceUseCase _updateAppearance;
        private readonly UpdateWidgetLayoutUseCase _updateLayout;
        private readonly UpdateWidgetTitleUseCase _updateTitle;
        private readonly GetStatUseCase _getStat;

        // ── Command ───────────────────────────────────────────────────────────
        public ICommand<Rect> UpdateLayoutCommand { get; }
        public ICommand<string> UpdateTitleCommand { get; }
        public ICommand<AppearanceDTO> UpdateAppearanceCommand { get; }
        public event Action<WidgetViewModel> OnSelected;

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
            UpdateWidgetAppearanceUseCase updateAppearance,
            UpdateWidgetLayoutUseCase updateLayout,
            UpdateWidgetTitleUseCase updateTitle,
            GetStatUseCase getStat)
        {
            _widget = widget;

            _updateAppearance = updateAppearance;
            _updateLayout = updateLayout;
            _updateTitle = updateTitle;
            _getStat = getStat;

            foreach (var binding in widget.Stats)
                LoadStat(binding);

            _widget.OnLayoutChanged += HandleLayoutChanged;
            UpdateLayoutCommand = new SimpleCommand<Rect>(rec => { updateLayout.Execute(_widget, rec); });

            _widget.OnAppearanceChanged += HandleAppearanceChanged;
            UpdateAppearanceCommand = new SimpleCommand<AppearanceDTO>(appearance =>
            {
                updateAppearance.Execute(_widget, appearance);
            });

            _widget.OnTitleChanged += HandleTitleChanged;
            UpdateTitleCommand = new SimpleCommand<string>(text => { updateTitle.Execute(_widget, text); });

            _widget.OnStatAdded += HandleStatAdded;
            _widget.OnStatRemoved += HandleStatRemoved;
            _widget.OnContentChanged += HandleContentChanged;
        }

        private void LoadStat(WidgetStatBinding stat)
        {
            BoundStats.Add(new StatViewModel(stat, _getStat.Execute(stat.StatId)));
        }

        private void HandleLayoutChanged() => RaisePropertyChanged(nameof(Layout));

        private void HandleAppearanceChanged()
        {
            RaisePropertyChanged(nameof(HasBorder));
            RaisePropertyChanged(nameof(BorderThickness));
            RaisePropertyChanged(nameof(BorderColor));
            RaisePropertyChanged(nameof(BackgroundColor));
            RaisePropertyChanged(nameof(BackgroundImagePath));
        }

        private void HandleTitleChanged() => RaisePropertyChanged(nameof(Title));

        private void HandleStatAdded(WidgetStatBinding stat)
        {
            LoadStat(stat);
        }

        private void HandleStatRemoved(WidgetStatBinding stat)
        {
            var boundStat = BoundStats.FirstOrDefault(s => s.Id == stat.StatId);
            if (boundStat != null) {
                BoundStats.Remove(boundStat);
                boundStat.Dispose();
            }
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