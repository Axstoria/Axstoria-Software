using CharacterSheet.App.UseCase;
using CharacterSheet.Domain.Widgets;

namespace CharacterSheet.Presenter.ViewModel.Widgets
{
    public class PointGaugeViewModel : WidgetViewModel
    {
        private readonly PointGaugeWidget _gauge;

        public PointGaugeViewModel(PointGaugeWidget widget, 
            UpdateWidgetAppearanceUseCase appearance, 
            UpdateWidgetLayoutUseCase updateLayout, 
            UpdateWidgetTitleUseCase updateTitle,
            GetStatUseCase getStat,
            UpdatePointGaugeWidgetUseCase up) : base(widget, appearance, updateLayout, updateTitle, getStat)
        {
            _gauge = widget;
        }

        protected override void HandleContentChanged()
        {
            
        }
    }
}