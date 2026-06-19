using System;
using CharacterSheet.App.UseCase;
using CharacterSheet.Domain;
using Loxodon.Framework.Commands;
using Loxodon.Framework.Observables;
using UnityEngine;

namespace CharacterSheet.Presenter.ViewModel
{
    public abstract class WidgetViewModel : ObservableObject
    {
        protected readonly SheetWidget _widget;
        
        public string Id => _widget.Id;
        
        protected UpdateWidgetAppearanceUseCase updateAppearance;
        protected UpdateWidgetLayoutUseCase updateLayout;
        
        // ── Command ───────────────────────────────────────────────────────────
        public ICommand UpdateLayoutCommand { get; }
        
        public event Action<Rect> OnLayoutChanged;
        public event Action<WidgetViewModel> OnSelected;
        
        public void Select() => OnSelected?.Invoke(this);
        
        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set => Set(ref _isSelected, value); 
        }

        private Rect _layout;
        public Rect Layout
        {
            get => _layout;
            set
            {
                if (_layout.Equals(value)) return;

                _layout = value;
                _widget.Layout = value;
                OnLayoutChanged?.Invoke(_layout);
                RaisePropertyChanged(nameof(Layout));
            }
        }

        private bool _hasBorder;
        public bool HasBorder
        {
            get => _hasBorder;
            set
            {
                Set(ref _hasBorder, value);
                _widget.HasBorder = value;
            }
        }
        
        private float _borderThickness;
        public float BorderThickness
        {
            get => _borderThickness;
            set
            {
                Set(ref _borderThickness, value);
                _widget.BorderThickness = value;
            }
        }
        
        private Color _borderColor;
        public Color BorderColor
        {
            get => _borderColor;
            set
            {
                Set(ref _borderColor, value);
                _widget.BorderColor = value;
            }
        }
        
        private Color _backgroundColor;
        public Color BackgroundColor
        {
            get => _backgroundColor;
            set
            {
                Set(ref _backgroundColor, value);
                _widget.BackgroundColor = value;
            }
        }

        public ObservableList<WidgetStatBinding> BoundStats { get; } = new();

        private readonly Action<Rect> _onLayoutChanged;
        
        protected WidgetViewModel(SheetWidget widget, UpdateWidgetAppearanceUseCase updateAppearance, UpdateWidgetLayoutUseCase updateLayout)
        {
            _widget = widget;
            
            _layout = widget.Layout;
            _hasBorder = widget.HasBorder;
            _borderThickness = widget.BorderThickness;
            _borderColor = widget.BorderColor;
            _backgroundColor = widget.BackgroundColor;
            
            this.updateAppearance = updateAppearance;
            this.updateLayout = updateLayout;
            
            foreach (var binding in widget.Stats)
                BoundStats.Add(binding);

            _onLayoutChanged = rect =>
            {
                _layout = rect;
                RaisePropertyChanged(nameof(Layout));
            };

            UpdateLayoutCommand = new SimpleCommand<Rect>(rec =>
            {
                Debug.Log(rec);
                updateLayout.Execute(_widget, rec);
                Layout = _widget.Layout;
            });
        }

        public void Dispose()
        {
            
        }
    }
}
