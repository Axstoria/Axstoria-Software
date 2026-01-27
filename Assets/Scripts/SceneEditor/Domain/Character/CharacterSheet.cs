using System.Collections.Generic;
using System.Drawing;

namespace Domain.Character
{
    public class CharacterSheet
    {
        public string Id { get; }
        public List<SheetWidget> Widgets { get; } = new();
        
        public bool HasBorder { get; set; }
        public float BorderThickness { get; set; }
        public Color BorderColor { get; set; }
        public Color BackgroundColor { get; set; }
        public string BackgroundImagePath { get; set; }
        
        public CharacterSheet(string id)
        {
            Id = id;
        }
    }
}