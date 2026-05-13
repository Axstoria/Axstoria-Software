using UnityEngine;

namespace CharacterSheet.App.DTO
{
    public class AppearanceDTO
    {
        public bool HasBorder { get; set; }
        public float BorderThickness { get; set; }
        public Color BorderColor { get; set; }
        public Color BackgroundColor { get; set; }
        public string BackgroundImagePath { get; set; }
    }
}