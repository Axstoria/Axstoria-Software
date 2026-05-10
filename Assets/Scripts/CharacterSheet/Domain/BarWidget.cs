using UnityEngine;

namespace CharacterSheet.Domain
{
    public class BarWidget :  SheetWidget
    {
        public int Segements { get; set; }
        public bool StackBars { get; set; } = true;
        
        public BarWidget(string id, Rect layout) : base(id, layout) {}
    }
}