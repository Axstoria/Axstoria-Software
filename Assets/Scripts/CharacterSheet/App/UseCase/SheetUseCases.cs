using AssetImporter.AssetImporter.App;
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
}