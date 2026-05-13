using CharacterSheet.Domain;

namespace CharacterSheet.Presenter.ViewModel
{
    public class WidgetViewModelFactory
    {
        public static WidgetViewModel Create(SheetWidget widget)
        {
            return new PointGaugeViewModel(
                new PointGaugeWidget(widget.Id, widget.Layout));
        }
    }
}