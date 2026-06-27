using CharacterSheet.App.DTO;
using CharacterSheet.Domain;
using UnityEngine;

namespace CharacterSheet.App.UseCase
{
    public class UpdateWidgetLayoutUseCase
    {
        public void Execute(SheetWidget widget, Rect dto)
        {
            if (dto.width < 100)
                dto.width = 200;
            widget.Layout = dto;
        }
    }
    
    public class UpdateWidgetAppearanceUseCase
    {
        public void Execute(SheetWidget widget, AppearanceDTO dto) {
            if (dto.HasBorder.HasValue)
                widget.HasBorder       = dto.HasBorder.Value;
            if (dto.BorderThickness.HasValue)
                widget.BorderThickness = dto.BorderThickness.Value;
            if (dto.BorderColor.HasValue)
                widget.BorderColor     = dto.BorderColor.Value;
            if (dto.BackgroundColor.HasValue)
                widget.BackgroundColor = dto.BackgroundColor.Value;
            if (dto.BackgroundImagePath != null)
            {
                widget.BackgroundImagePath = dto.BackgroundImagePath;
            }
            //widget.OnAppearanceChanged?.Invoke();
        }
    }

    public class UpdateWidgetTitleUseCase
    {
        public void Execute(SheetWidget widget, string text)
        {
            if (widget == null) return;
            widget.Title = text;
        }
    }
}