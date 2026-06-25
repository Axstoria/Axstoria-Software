using CharacterSheet.App.DTO;
using CharacterSheet.Domain;

namespace CharacterSheet.App.UseCase
{
    public class UpdateWidgetAppearanceUseCase
    {
        public void Execute(SheetWidget widget, AppearanceDTO dto) {
            if (dto.HasBorder.HasValue)
                widget.HasBorder       = dto.HasBorder.Value;
            if (dto.BorderColor.HasValue)
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
}