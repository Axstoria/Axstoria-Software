using UnityEngine;

namespace CharacterSheet.Domain
{
    public interface IAppearanceTarget
    {
        bool HasBorder { get; set; }
        float BorderThickness { get; set; }
        Color BorderColor { get; set; }
        Color BackgroundColor { get; set; }
        string BackgroundImagePath { get; set; }
    }
}