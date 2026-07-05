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
        public int Segments { get; set; }
        public bool StackBars { get; set; } = true;
    }
}