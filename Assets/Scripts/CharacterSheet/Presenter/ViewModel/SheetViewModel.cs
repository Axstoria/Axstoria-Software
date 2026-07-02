using System;
using System.Linq;
using AssetImporter.AssetImporter.App.UseCase;
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

        private readonly IImageLoaderService _imageLoader;
        // ── Use Case ───────────────────────────────────────────────────────────
        private readonly UpdateAppearanceUseCase _updateSheet;
        private readonly UpdateBackgroundUseCase _updateBackground;
        private readonly AddWidgetUseCase _addWidget;
        private readonly RemoveWidgetUseCase _removeWidget;

        // ── Command ───────────────────────────────────────────────────────────
        public ICommand AddWidgetCommand { get; }
        public ICommand<string> RemoveWidgetCommand { get; }
        public event Action<WidgetViewModel> OnWidgetSelected;
        public ICommand<AppearanceDTO> UpdateAppearanceCommand { get; }
        public ICommand SelectBackgroundCommand { get; }
        
        private readonly Action<SheetWidget> _onWidgetAdded;
        private readonly Action<SheetWidget> _onWidgetRemoved;

        public ObservableList<WidgetViewModel> Widgets { get; } = new();

        public bool HasBorder => _sheet.HasBorder;
        public float BorderThickness => _sheet.BorderThickness;
        public Color BorderColor => _sheet.BorderColor;
        public Color BackgroundColor => _sheet.BackgroundColor;
        private Sprite _backgroundSprite;

        public Sprite BackgroundSprite
        {
            get => _backgroundSprite;
            private set
            {
                _backgroundSprite = value; 
                RaisePropertyChanged();
            }
        }

        public SheetViewModel(Sheet sheet,
            WidgetViewModelFactory widgetFactory,
            UpdateAppearanceUseCase updateSheet,
            UpdateBackgroundUseCase updateBackground,
            IImageLoaderService imageService,
            AddWidgetUseCase addWidget,
            RemoveWidgetUseCase removeWidget)
        {
            _sheet = sheet;
            _factory = widgetFactory;
            _imageLoader  = imageService;

            _updateSheet = updateSheet;
            _updateBackground = updateBackground;
            _addWidget = addWidget;
            _removeWidget = removeWidget;

            foreach (var widget in sheet.Widgets) {
                var vm = widgetFactory.Create(widget);
                vm.OnSelected += HandleWidgetSelected;
                Widgets.Add(vm);
            }

            if (!string.IsNullOrEmpty(_sheet.BackgroundImagePath))
                LoadSprite(_sheet.BackgroundImagePath);

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

            _sheet.OnPathChanged += LoadSprite;
            SelectBackgroundCommand = new SimpleCommand<object>(_ =>
            {
                _updateBackground.Execute(_sheet, Id);
            });

            sheet.OnWidgetAdded += _onWidgetAdded;
            sheet.OnWidgetRemoved += _onWidgetRemoved;

            AddWidgetCommand = new SimpleCommand<WidgetType>(type => { addWidget.Execute(_sheet, type); });

            RemoveWidgetCommand = new SimpleCommand<string>(id => { _removeWidget.Execute(_sheet, id); });
        }

        private void HandleWidgetSelected(WidgetViewModel widgetVM)
        {
            OnWidgetSelected?.Invoke(widgetVM);
        }

        private void LoadSprite(string path)
        {
            if (_backgroundSprite != null) {
                UnityEngine.Object.Destroy(_backgroundSprite.texture);
                UnityEngine.Object.Destroy(_backgroundSprite);
            }

            BackgroundSprite = _imageLoader.LoadSprite(path, Id);
        }

        private void HandleAppearanceChanged()
        {
            RaisePropertyChanged(nameof(HasBorder));
            RaisePropertyChanged(nameof(BorderThickness));
            RaisePropertyChanged(nameof(BorderColor));
            RaisePropertyChanged(nameof(BackgroundColor));
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

            _sheet.OnPathChanged -= LoadSprite;
            _sheet.OnAppearanceChanged -= HandleAppearanceChanged;

            /*foreach (var vm in Stats) vm.Dispose();*/
            foreach (var vm in Widgets) vm.Dispose();
        }
    }
}