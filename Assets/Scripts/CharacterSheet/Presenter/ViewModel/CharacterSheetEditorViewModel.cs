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
        private SheetViewModel currentSheet;

        public SheetViewModel CurrentSheet
        {
            get => currentSheet;
            private set => Set(ref currentSheet, value);
        }

        private WidgetViewModel selectedWidget;

        public WidgetViewModel SelectedWidget
        {
            get => selectedWidget;
            private set => Set(ref selectedWidget, value);
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
        public ICommand<string> AddStatCommand { get; }

        public CharacterSheetEditorViewModel(SheetViewModelFactory factory, LoadUseCases load, SaveUseCases save)
        {
            _loadSheet = load;
            _saveSheet = save;
            _sheetFactory = factory;

            string lastSheetId = PlayerPrefs.GetString("LastOpenedSheetId", string.Empty);

            var sheet = _loadSheet.Execute(lastSheetId);
            if (sheet != null)
                LoadSheet(_sheetFactory.Create(sheet));
            else
                LoadSheet(_sheetFactory.Create(new Sheet()));

            AddWidgetCommand = new SimpleCommand<WidgetType>(type => { currentSheet?.AddWidgetCommand.Execute(type); });

            RemoveWidgetCommand = new SimpleCommand<object>(_ =>
            {
                if (selectedWidget != null) {
                    currentSheet?.RemoveWidgetCommand.Execute(selectedWidget.Id);
                    selectedWidget = null;
                }
            });

            SaveSheetCommand = new SimpleCommand<object>(_ =>
            {
                SaveCurrentSheet();
            });

            AddStatCommand = new SimpleCommand<String>(id =>
            {
                if (currentSheet != null)
                    currentSheet.AddStatCommand.Execute(id);
            });
        }

        public void SaveCurrentSheet()
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