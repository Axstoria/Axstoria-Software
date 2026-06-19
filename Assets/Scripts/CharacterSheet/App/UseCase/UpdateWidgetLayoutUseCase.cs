using CharacterSheet.App.DTO;
using CharacterSheet.Domain;
using UnityEngine;

namespace CharacterSheet.App.UseCase
{
    public class UpdateWidgetLayoutUseCase
    {
        public void Execute(SheetWidget widget, Rect dto)
        {
            Debug.Log(dto);
            if (dto.width < 100)
                dto.width = 80;
            widget.Layout = dto;
        }
    }
}