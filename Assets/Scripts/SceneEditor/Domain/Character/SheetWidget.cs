using System.Collections.Generic;
using UnityEngine;

namespace Domain.Character
{
    public abstract class SheetWidget
    {
        public string Id { get; }
        public Rect Layout { get; set; }
        
        public List<WidgetStatBinding> Stats { get; } = new();
        
        public bool HasBorder { get; set; }
        public float BorderThickness { get; set; }
        public Color BorderColor { get; set; }
        public Color BackgroundColor { get; set; }
        public string BackgroundImagePath { get; set; }
        
        protected SheetWidget(string id)
        {
            Id = id;
        }
    }
}