using AssetImporter.AssetImporter.App;
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

    public class UpdateWidgetTitleUseCase
    {
        public void Execute(SheetWidget widget, string text)
        {
            if (widget == null) return;
            widget.Title = text;
        }
    }
}