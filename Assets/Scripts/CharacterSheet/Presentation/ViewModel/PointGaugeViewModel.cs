using CharacterSheet.Domain;

namespace CharacterSheet.Presentation.ViewModel
{
    public class PointGaugeViewModel : WidgetViewModel
    {
        private readonly PointGaugeWidget _pointGaugeWidget;

        private int _maxPoints;

        public int MaxPoints
        {
            get => _maxPoints;
            set
            {
                Set(ref _maxPoints, value);
                _pointGaugeWidget.MaxPoints = _maxPoints;
            }
        }

        public PointGaugeViewModel(PointGaugeWidget pointGaugeWidget) : base(pointGaugeWidget)
        {
            _pointGaugeWidget = pointGaugeWidget;
            _maxPoints = _pointGaugeWidget.MaxPoints;
        }
    }
}