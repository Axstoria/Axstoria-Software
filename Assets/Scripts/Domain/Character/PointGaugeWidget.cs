namespace Domain.Character
{
    public class PointGaugeWidget : SheetWidget
    {
        public int MaxPoints { get; set; }
        public bool FillByValue { get; set; } = true;

        public PointGaugeWidget(string id) : base(id) {}
    }
}