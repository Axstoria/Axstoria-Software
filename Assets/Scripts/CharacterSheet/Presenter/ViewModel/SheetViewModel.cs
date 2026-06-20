using System;
using System.Linq;
using CharacterSheet.App.UseCase;
using CharacterSheet.Domain;
using Loxodon.Framework.Commands;
using Loxodon.Framework.Observables;
using UnityEngine;

namespace CharacterSheet.Presenter.ViewModel
{
    public class SheetViewModel : ObservableObject
    {
        private readonly Sheet _sheet;
        private readonly WidgetViewModelFactory _factory;
        
        public string Id => _sheet.Id;

        // ── Use Case ───────────────────────────────────────────────────────────
        private readonly AddStatUseCase addStat;
        private readonly RemoveStatUseCase removeStat;
        private readonly UpdateSheetUseCase updateSheet;
        private readonly AddWidgetUseCase addWidget;
        private readonly RemoveWidgetUseCase removeWidget;
        private readonly BindStatToWidgetUseCase bindStatToWidget;
        private readonly UnbindStatUseCase unbindStat;
        
        // ── Command ───────────────────────────────────────────────────────────
        public ICommand AddWidgetCommand { get; }
        public ICommand AddStatCommand { get; }

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
        
        public event Action<WidgetViewModel> OnWidgetSelected;

        public SheetViewModel(Sheet sheet, 
            WidgetViewModelFactory widgetFactory,
            AddStatUseCase addStat,
            RemoveStatUseCase removeStat,
            UpdateSheetUseCase updateSheet,
            AddWidgetUseCase addWidget,
            RemoveWidgetUseCase removeWidget,
            BindStatToWidgetUseCase bindStatToWidget,
            UnbindStatUseCase unbindStat)
        {
            _sheet = sheet;
            _factory = widgetFactory;
            
            _hasBorder = sheet.HasBorder;
            _borderThickness = sheet.BorderThickness;
            _borderColor = sheet.BorderColor;
            _backgroundColor = sheet.BackgroundColor;

            this.addStat = addStat;
            this.removeStat = removeStat;
            this.updateSheet = updateSheet;
            this.addWidget = addWidget;
            this.removeWidget = removeWidget;
            this.bindStatToWidget = bindStatToWidget;
            this.unbindStat = unbindStat;
            
            foreach (var widget in sheet.Widgets)
            {
                var vm = widgetFactory.Create(widget);
                vm.OnSelected += OnWidgetSelected;
                Widgets.Add(vm);
            }
            
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
            _onWidgetAdded = widget =>
            {
                var vm = widgetFactory.Create(widget);
                vm.OnSelected += OnWidgetSelected;
                Widgets.Add(vm);
            };
            _onWidgetRemoved = widget =>
            {
                var vm = Widgets.FirstOrDefault(w => w.Id == widget.Id); 
                if (vm != null)
                {
                    vm.OnSelected -= OnWidgetSelected;
                    vm.Dispose();
                    Widgets.Remove(vm);
                }
            };

            sheet.OnStatAdded     += _onStatAdded;
            sheet.OnStatRemoved   += _onStatRemoved;
            sheet.OnWidgetAdded   += _onWidgetAdded;
            sheet.OnWidgetRemoved += _onWidgetRemoved;

            AddWidgetCommand = new SimpleCommand<WidgetType>(type =>
            {
                Rect defaultLayout = new Rect(0, 0, 200, 50);
                addWidget.Execute(_sheet, type, defaultLayout);
            });

            AddStatCommand = new SimpleCommand<String>(id =>
            {
                addStat.Execute(_sheet, id);
            });
        }

        public void ClearSelection()
        {
            OnWidgetSelected?.Invoke(null);
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
    }
}
