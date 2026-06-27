using System;
using CharacterSheet.App.UseCase;
using CharacterSheet.Domain;
using Loxodon.Framework.Commands;
using Loxodon.Framework.ViewModels;
using UnityEngine;

namespace CharacterSheet.Presenter.ViewModel
{
    public class CharacterSheetEditorViewModel : ViewModelBase
    {
        // ── Sub-ViewModels ────────────────────────────────────────────────────
        private SheetViewModel _currentSheet;

        public SheetViewModel CurrentSheet
        {
            get => _currentSheet;
            private set => Set(ref _currentSheet, value);
        }

        private WidgetViewModel _selectedWidget;

        public WidgetViewModel SelectedWidget
        {
            get => _selectedWidget;
            private set => Set(ref _selectedWidget, value);
        }

        private readonly SheetViewModelFactory _sheetFactory;

        // ── Use Case ──────────────────────────────────────────────────────────
        private readonly LoadUseCases _loadSheet;

        private readonly SaveUseCases _saveSheet;

        // ── Command ───────────────────────────────────────────────────────────
        public ICommand UndoCommand { get; }
        public ICommand RedoCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand<WidgetType> AddWidgetCommand { get; }
        public ICommand RemoveWidgetCommand { get; }
        public ICommand SaveSheetCommand { get; }

        public CharacterSheetEditorViewModel(SheetViewModelFactory factory, LoadUseCases load, SaveUseCases save)
        {
            _loadSheet = load;
            _saveSheet = save;
            _sheetFactory = factory;

            string lastSheetId = PlayerPrefs.GetString("LastOpenedSheetId", string.Empty);

            var sheet = _loadSheet.Execute(lastSheetId);
            LoadSheet(sheet != null ? _sheetFactory.Create(sheet) : _sheetFactory.Create(new Sheet()));

            AddWidgetCommand = new SimpleCommand<WidgetType>(type =>
            {
                _currentSheet?.AddWidgetCommand.Execute(type);
            });

            RemoveWidgetCommand = new SimpleCommand<object>(_ =>
            {
                if (_selectedWidget == null) return;
                _currentSheet?.RemoveWidgetCommand.Execute(_selectedWidget.Id);
                _selectedWidget = null;
            });

            SaveSheetCommand = new SimpleCommand<object>(_ => { SaveCurrentSheet(); });
        }

        private void SaveCurrentSheet()
        {
            if (CurrentSheet == null) return;

            _saveSheet.Execute(CurrentSheet.RuntimeSheet);

            PlayerPrefs.SetString("LastOpenedSheetId", CurrentSheet.Id);
            PlayerPrefs.Save();
        }

        private void LoadSheet(SheetViewModel sheet)
        {
            CurrentSheet?.Dispose();
            CurrentSheet = sheet;

            CurrentSheet.OnWidgetSelected += vm =>
            {
                if (SelectedWidget == vm) return;

                if (SelectedWidget != null) SelectedWidget.IsSelected = false;

                SelectedWidget = vm;

                if (SelectedWidget != null) SelectedWidget.IsSelected = true;
            };
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) {
                SaveCurrentSheet();
                CurrentSheet?.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}