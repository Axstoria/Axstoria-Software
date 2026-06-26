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
    public class SheetViewModel : ObservableObject
    {
        private readonly Sheet _sheet;
        public Sheet RuntimeSheet => _sheet;
        private readonly WidgetViewModelFactory _factory;

        public string Id => _sheet.Id;

        // ── Use Case ───────────────────────────────────────────────────────────
        private readonly AddStatUseCase _addStat;
        private readonly RemoveStatUseCase _removeStat;
        private readonly UpdateSheetUseCase _updateSheet;
        private readonly AddWidgetUseCase _addWidget;
        private readonly RemoveWidgetUseCase _removeWidget;
        private readonly BindStatToWidgetUseCase _bindStatToWidget;
        private readonly UnbindStatUseCase _unbindStat;

        // ── Command ───────────────────────────────────────────────────────────
        public ICommand AddWidgetCommand { get; }
        public ICommand<string> RemoveWidgetCommand { get; }
        public ICommand AddStatCommand { get; }

        /*public ObservableList<StatViewModel> Stats { get; } = new();*/
        public ObservableList<WidgetViewModel> Widgets { get; } = new();

        public bool HasBorder => _sheet.HasBorder;
        public float BorderThickness => _sheet.BorderThickness;
        public Color BorderColor => _sheet.BorderColor;
        public Color BackgroundColor => _sheet.BackgroundColor;
        public string BackgroundImagePath => _sheet.BackgroundImagePath;

        /*private readonly Action<StatValue> _onStatAdded;
        private readonly Action<StatValue> _onStatRemoved;*/
        private readonly Action<SheetWidget> _onWidgetAdded;
        private readonly Action<SheetWidget> _onWidgetRemoved;

        public event Action<WidgetViewModel> OnWidgetSelected;
        public ICommand<AppearanceDTO> UpdateAppearanceCommand { get; }

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

            _addStat = addStat;
            _removeStat = removeStat;
            _updateSheet = updateSheet;
            _addWidget = addWidget;
            _removeWidget = removeWidget;
            _bindStatToWidget = bindStatToWidget;
            _unbindStat = unbindStat;

                foreach (var widget in sheet.Widgets) {
                    var vm = widgetFactory.Create(widget);
                    vm.OnSelected += HandleWidgetSelected;
                    Widgets.Add(vm);
                }
                

                _onWidgetAdded = widget =>
                {
                    var vm = widgetFactory.Create(widget);
                    vm.OnSelected += HandleWidgetSelected;
                    Widgets.Add(vm);
                };
                _onWidgetRemoved = widget =>
                {
                    var vm = Widgets.FirstOrDefault(w => w.Id == widget.Id);
                    if (vm != null) {
                        vm.OnSelected -= HandleWidgetSelected;
                        vm.Dispose();
                        Widgets.Remove(vm);
                    }
                };

            _sheet.OnAppearanceChanged += HandleAppearanceChanged;
            UpdateAppearanceCommand = new SimpleCommand<AppearanceDTO>(appearance =>
            {
                updateSheet.Execute(_sheet, appearance);
            });

            /*sheet.OnStatAdded     += _onStatAdded;
            sheet.OnStatRemoved   += _onStatRemoved;*/
            sheet.OnWidgetAdded += _onWidgetAdded;
            sheet.OnWidgetRemoved += _onWidgetRemoved;

            AddWidgetCommand = new SimpleCommand<WidgetType>(type =>
            {
                addWidget.Execute(_sheet, type);
            });
            
            RemoveWidgetCommand = new SimpleCommand<string>(id =>
            {
                _removeWidget.Execute(_sheet, id);
            });

            AddStatCommand = new SimpleCommand<String>(id => { addStat.Execute(_sheet, id); });
        }
        
        private void HandleWidgetSelected(WidgetViewModel widgetVM)
        {
            OnWidgetSelected?.Invoke(widgetVM);
        }

        private void HandleAppearanceChanged()
        {
            RaisePropertyChanged(nameof(HasBorder));
            RaisePropertyChanged(nameof(BorderThickness));
            RaisePropertyChanged(nameof(BorderColor));
            RaisePropertyChanged(nameof(BackgroundColor));
            RaisePropertyChanged(nameof(BackgroundImagePath));
        }

        public void ClearSelection()
        {
            OnWidgetSelected?.Invoke(null);
        }

        public void Dispose()
        {
            /*_sheet.OnStatAdded     -= _onStatAdded;
            _sheet.OnStatRemoved   -= _onStatRemoved;*/
            _sheet.OnWidgetAdded -= _onWidgetAdded;
            _sheet.OnWidgetRemoved -= _onWidgetRemoved;

            _sheet.OnAppearanceChanged -= HandleAppearanceChanged;

            /*foreach (var vm in Stats) vm.Dispose();*/
            foreach (var vm in Widgets) vm.Dispose();
        }
    }
}