using CharacterSheet.App.UseCase;
using CharacterSheet.Domain.Widgets;
using Loxodon.Framework.Commands;

namespace CharacterSheet.Presenter.ViewModel.Widgets
{
    public class TextWidgetViewModel : WidgetViewModel
    {
        private readonly TextWidget _gauge;

        // ── Use Case ───────────────────────────────────────────────────────────
        private readonly UpdateTextWidgetUseCase _updateText;

        // ── Command ────────────────────────────────────────────────────────────
        public ICommand<string> UpdateContentCommand { get; }

        // ── Exposed Attribute ──────────────────────────────────────────────────
        public string TextTemplate => _gauge.TextTemplate;
        private string _displayedText;

        public string DisplayedText
        {
            get => _displayedText;
            private set => Set(ref _displayedText, value);
        }

        public TextWidgetViewModel(TextWidget widget,
            BindStatToWidgetUseCase bindStatToWidgetUseCase,
            UpdateAppearanceUseCase appearance,
            UpdateBackgroundUseCase  background,
            UpdateWidgetLayoutUseCase updateLayout,
            UpdateWidgetTitleUseCase updateTitle,
            GetStatUseCase getStat,
            UpdateTextWidgetUseCase up) : base(widget, bindStatToWidgetUseCase, appearance, background, updateLayout, updateTitle, getStat)
        {
            _gauge = widget;
            _updateText = up;

            UpdateContentCommand = new SimpleCommand<string>(text => { _updateText.Execute(_gauge, text); });

            this.BoundStats.CollectionChanged += (s, e) => RefreshDisplayedText();
            RefreshDisplayedText();
        }

        private void RefreshDisplayedText()
        {
            if (string.IsNullOrEmpty(TextTemplate)) {
                DisplayedText = string.Empty;
                return;
            }

            string result = TextTemplate;
            result = result.Replace("{0}", "NULL");
            foreach (var stat in BoundStats) {
                string placeholder = $"{{{stat.Id}}}";

                if (result.Contains(placeholder)) {
                    result = result.Replace(placeholder, stat.CurrentValue.ToString());
                }
            }

            DisplayedText = result;
        }

        protected override void HandleContentChanged()
        {
            RefreshDisplayedText();
            RaisePropertyChanged(nameof(TextTemplate));
            RaisePropertyChanged(nameof(DisplayedText));
        }
    }
}