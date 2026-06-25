using CharacterSheet.App.DTO;
using CharacterSheet.Domain;

namespace CharacterSheet.App.UseCase
{
    public class UpdateSheetUseCase
    {
        public void Execute(Sheet sheet, AppearanceDTO dto) {
            if (dto.HasBorder.HasValue)
                sheet.HasBorder       = dto.HasBorder.Value;
            if (dto.BorderColor.HasValue)
                sheet.BorderThickness = dto.BorderThickness.Value;
            if (dto.BorderColor.HasValue)
                sheet.BorderColor     = dto.BorderColor.Value;
            if (dto.BackgroundColor.HasValue)
                sheet.BackgroundColor = dto.BackgroundColor.Value;
            if (dto.BackgroundImagePath != null)
            {
                sheet.BackgroundImagePath = dto.BackgroundImagePath;
            }
            //sheet.OnAppearanceChanged?.Invoke();
        }
    }
}