using UnityEngine;

namespace CharacterSheet.Domain.Widgets
{
    /*enum Direction
    {
        Horizontal,
        Vertical,
        Circular
    }*/
    public class BarWidget :  SheetWidget
    {
        //public Direction dir = Direction.Horizontal;
        public bool ShowValue { get; set; } = true;
        public int Segments { get; set; }
        public bool StackBars { get; set; } = true;
        
        public BarWidget(string id, Rect layout) : base(id, layout) {}
    }
}