using CharacterSheet.App.DTO;
using CharacterSheet.Domain;

namespace CharacterSheet.App.UseCase
{
    public class AddWidgetUseCase
    {
        public string Execute(Sheet sheet, WidgetType type)
        {
            var widget = WidgetFactory.Create(type);
            sheet.AddWidget(widget);
            return widget.Id;
        }
    }
    
    public class RemoveWidgetUseCase
    {
        public void Execute(Sheet sheet, string id)
        {
            if (!sheet.HasWidget(id)) return;
            sheet.RemoveWidget(id);
        }
    }
    
    public class UpdateSheetUseCase
    {
        public void Execute(Sheet sheet, AppearanceDTO dto) {
            if (dto.HasBorder.HasValue)
                sheet.HasBorder       = dto.HasBorder.Value;
            if (dto.BorderThickness.HasValue)
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