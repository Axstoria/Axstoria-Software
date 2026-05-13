using CharacterSheet.App.UseCase;
using CharacterSheet.Domain;
using Loxodon.Framework.ViewModels;
using UnityEngine;

namespace CharacterSheet.Presenter.ViewModel
{
    public class CharacterSheetEditorViewModel : ViewModelBase
    {
        // ── Sub-ViewModels ────────────────────────────────────────────────────
        public SheetViewModel    Sheet    { get; private set; }
        // ── Observable state ──────────────────────────────────────────────────
        // ── Use cases ─────────────────────────────────────────────────────────
        public AddStatUseCase AddStat { get; }
        public RemoveStatUseCase RemoveStat { get; }
        public BindStatToWidgetUseCase BindStatToWidget { get; }
        public UnbindStatUseCase UnbindStat { get; }
        public AddWidgetUseCase AddWidget { get; }
        public RemoveWidgetUseCase RemoveWidget { get; }

        public CharacterSheetEditorViewModel(Sheet sheet,
            AddStatUseCase addStat,
            RemoveStatUseCase removeStat,
            BindStatToWidgetUseCase bindStatToWidget,
            UnbindStatUseCase unbindStat,
            AddWidgetUseCase addWidget,
            RemoveWidgetUseCase removeWidget)
        {
            AddStat = addStat;
            RemoveStat = removeStat;
            BindStatToWidget = bindStatToWidget;
            UnbindStat = unbindStat;
            AddWidget = addWidget;
            RemoveWidget = removeWidget;
        }

        public void LoadSheet(Sheet sheet)
        {
            
        }
    }
}
