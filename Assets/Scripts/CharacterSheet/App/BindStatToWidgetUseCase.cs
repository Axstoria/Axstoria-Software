using System.Linq;
using CharacterSheet.Domain;

namespace CharacterSheet.App
{
    public class BindStatToWidgetUseCase
    {
        public void Execute(Sheet sheet, string widgetId, string statId)
        {
            if (!sheet.HasStat(statId)) return;
            var widget = sheet.Widgets.FirstOrDefault(w => w.Id == widgetId);
            if (widget == null) return;
            widget.Stats.Add(new WidgetStatBinding(statId));
        }
    } 
}