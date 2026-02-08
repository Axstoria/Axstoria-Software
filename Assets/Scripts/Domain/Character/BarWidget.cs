namespace Domain.Character
{
    public class BarWidget :  SheetWidget
    {
        public int Segements { get; set; }
        public bool StackBars { get; set; } = true;
        
        public BarWidget(string id) : base(id) {}
    }
}