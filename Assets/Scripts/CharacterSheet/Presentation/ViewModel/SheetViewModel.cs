using System;
using System.Linq;
using CharacterSheet.Domain;
using Loxodon.Framework.Observables;
using UnityEngine;

namespace CharacterSheet.Presentation.ViewModel
{
    public class SheetViewModel : ObservableObject
    {
        private readonly Sheet _sheet;
        
        public string Id => _sheet.Id;

        public ObservableList<StatViewModel> Stats { get; } = new();
        public ObservableList<WidgetViewModel> Widgets { get; } = new();
        
        private bool _hasBorder;
        public bool HasBorder
        {
            get => _hasBorder;
            set
            {
                Set(ref _hasBorder, value);
                _sheet.HasBorder = value;
            }
        }

        private float _borderThickness;
        public float BorderThickness
        {
            get => _borderThickness;
            set
            {
                Set(ref _borderThickness, value);
                _sheet.BorderThickness = value;
            }
        }
        
        private Color _borderColor;
        public Color BorderColor
        {
            get => _borderColor;
            set
            {
                Set(ref _borderColor, value);
                _sheet.BorderColor = value;
            }
        }
        
        private Color _backgroundColor;
        public Color BackgroundColor
        {
            get => _backgroundColor;
            set
            {
                Set(ref _backgroundColor, value);
                _sheet.BackgroundColor = value;
            }
        }
        
        private readonly Action<StatValue> _onStatAdded;
        private readonly Action<StatValue> _onStatRemoved;
        private readonly Action<SheetWidget> _onWidgetAdded;
        private readonly Action<SheetWidget> _onWidgetRemoved;

        public SheetViewModel(Sheet sheet)
        {
            _sheet = sheet;
            
            _hasBorder = sheet.HasBorder;
            _borderThickness = sheet.BorderThickness;
            _borderColor = sheet.BorderColor;
            _backgroundColor = sheet.BackgroundColor;
            
            foreach (var stat in sheet.Stats)
                Stats.Add(new StatViewModel(stat));
            foreach (var widget in sheet.Widgets)
                Widgets.Add(new WidgetViewModel(widget));
            
            _onStatAdded     = stat   => Stats.Add(new StatViewModel(stat));
            _onStatRemoved   = stat =>
            {
                var vm = Stats.FirstOrDefault(s => s.Id == stat.Id); 
                if (vm != null)
                {
                    Stats.Remove(vm);
                    vm.Dispose();
                }
            };
            _onWidgetAdded   = widget => Widgets.Add(new WidgetViewModel(widget));
            _onWidgetRemoved = widget =>
            {
                var vm = Widgets.FirstOrDefault(w => w.Id == widget.Id); 
                if (vm != null)
                {
                    Widgets.Remove(vm);
                    vm.Dispose();
                }
            };

            sheet.OnStatAdded     += _onStatAdded;
            sheet.OnStatRemoved   += _onStatRemoved;
            sheet.OnWidgetAdded   += _onWidgetAdded;
            sheet.OnWidgetRemoved += _onWidgetRemoved;
        }

        public void Dispose()
        {
            _sheet.OnStatAdded     -= _onStatAdded;
            _sheet.OnStatRemoved   -= _onStatRemoved;
            _sheet.OnWidgetAdded   -= _onWidgetAdded;
            _sheet.OnWidgetRemoved -= _onWidgetRemoved;
            
            foreach (var vm in Stats) vm.Dispose();
            foreach (var vm in Widgets) vm.Dispose();
        }
        
        private WidgetViewModel CreateWidgetViewModel(SheetWidget widget) => widget switch {
            PointGaugeWidget g => new PointGaugeViewModel(g),
            /*BarWidget b        => new BarViewModel(b),
            TextWidget t       => new TextViewModel(t),*/
            _                  => throw new ArgumentException($"Unknown widget type: {widget.GetType()}")
        };
    }
}
