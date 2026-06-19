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
        // ── Observable state ──────────────────────────────────────────────────
        // ── Command ───────────────────────────────────────────────────────────
        public ICommand UndoCommand { get; }
        public ICommand RedoCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand<WidgetType> AddWidgetCommand { get; }
        public ICommand<string> AddStatCommand { get; }

        public CharacterSheetEditorViewModel(SheetViewModel sheet)
        {
            LoadSheet(sheet);
            sheet.OnWidgetSelected += vm => SelectedWidget = vm;
            
            AddWidgetCommand = new SimpleCommand<WidgetType>(type =>
            {
                Debug.Log("create widget");
                if (currentSheet != null)
                    currentSheet.AddWidgetCommand.Execute(type);
            });

            AddStatCommand = new SimpleCommand<String>(id =>
            {
                if (currentSheet != null)
                    currentSheet.AddStatCommand.Execute(id);
            });

            sheet.OnWidgetSelected += vm =>
            {
                if (SelectedWidget != null) SelectedWidget.IsSelected = false;
                
                SelectedWidget = vm;
                
                if (SelectedWidget != null) SelectedWidget.IsSelected = true;
            };
        }

        public void LoadSheet(SheetViewModel sheet)
        {
            CurrentSheet?.Dispose();
            CurrentSheet = sheet;
        }
    }
}
