using System;
using CharacterSheet.Domain;
using Loxodon.Framework.Observables;
using UnityEngine;

namespace CharacterSheet.Presentation.ViewModel
{
    public class WidgetViewModel : ObservableObject
    {
        private readonly SheetWidget _widget;
        
        public string Id => _widget.Id;

        private Rect _layout;
        public Rect Layout
        {
            get => _layout;
            set
            {
                Set(ref _layout, value, nameof(Layout));
                _widget.Layout = value;
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
        
        public WidgetViewModel(SheetWidget widget)
        {
            _widget = widget;
            
            _layout = widget.Layout;
            _hasBorder = widget.HasBorder;
            _borderThickness = widget.BorderThickness;
            _borderColor = widget.BorderColor;
            _backgroundColor = widget.BackgroundColor;
            
            foreach (var binding in widget.Stats)
                BoundStats.Add(binding);

            _onLayoutChanged = rect =>
            {
                _layout = rect;
                RaisePropertyChanged(nameof(Layout));
            };
            
            
        }

        public void Dispose()
        {
            
        }
    }
}
