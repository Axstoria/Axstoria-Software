using System.Collections.Generic;
using UnityEngine;

namespace CharacterSheet.Domain
{
    public abstract class SheetWidget
    {
        public string Id { get; }
        public Rect Layout { get; set; }
        
        public List<WidgetStatBinding> Stats { get; } = new();

        public bool HasBorder { get; set; } = false;
        public float BorderThickness { get; set; } = 0.1f;
        public Color BorderColor { get; set; } = Color.black;
        public Color BackgroundColor { get; set; } = Color.white;
        public string BackgroundImagePath { get; set; }
        
        protected SheetWidget(string id, Rect layout)
        {
            Id = id;
            Layout = layout;
        }
    }
}