using CharacterSheet.Domain;

namespace CharacterSheet.Presenter.ViewModel.Widgets
{
    public class PointGaugeViewModel : WidgetViewModel
    {
        private readonly PointGaugeWidget _gauge;

        private int _maxPoints;
        public int MaxPoints
        {
            get => _maxPoints;
            set
            {
                Set(ref _maxPoints, value);
                _gauge.MaxPoints = MaxPoints;
            }
        }
        
        private bool _fillByValue;
        public bool FillByValue
        {
            get => _fillByValue;
            set
            {
                Set(ref _fillByValue, value);
                _gauge.FillByValue = _fillByValue;
            }
        }

        public PointGaugeViewModel(PointGaugeWidget widget) : base(widget)
        {
            _gauge = widget;
            _maxPoints = widget.MaxPoints;
            _fillByValue = widget.FillByValue;
        }
    }
}