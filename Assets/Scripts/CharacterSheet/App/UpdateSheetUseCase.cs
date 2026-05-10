using CharacterSheet.Domain;

namespace CharacterSheet.App
{
    public class UpdateSheetUseCase
    {
        public void Execute(Sheet sheet, AppearanceDTO dto) {
            sheet.HasBorder       = dto.HasBorder;
            sheet.BorderThickness = dto.BorderThickness;
            sheet.BorderColor     = dto.BorderColor;
            sheet.BackgroundColor = dto.BackgroundColor;
            sheet.BackgroundImagePath = dto.BackgroundImagePath;
            //sheet.OnAppearanceChanged?.Invoke();
        }
    }
}