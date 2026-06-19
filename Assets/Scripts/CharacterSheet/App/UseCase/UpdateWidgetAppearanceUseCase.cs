using CharacterSheet.App.DTO;
using CharacterSheet.Domain;

namespace CharacterSheet.App.UseCase
{
    public class UpdateWidgetAppearanceUseCase
    {
        public void Execute(SheetWidget widget, AppearanceDTO dto) {
            widget.HasBorder       = dto.HasBorder;
            widget.BorderThickness = dto.BorderThickness;
            widget.BorderColor     = dto.BorderColor;
            //widget.OnAppearanceChanged?.Invoke();
        }
    }
}