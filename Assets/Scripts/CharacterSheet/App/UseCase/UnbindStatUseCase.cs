using System.Linq;
using CharacterSheet.Domain;

namespace CharacterSheet.App.UseCase
{
    public class UnbindStatUseCase
    {
        public void Execute(Sheet sheet, string widgetId, string statId)
        {
            if (!sheet.HasStat(statId)) return;
            var widget = sheet.Widgets.FirstOrDefault(w => w.Id == widgetId);
            if (widget == null) return;
            widget.Stats.RemoveAll(s => s.StatId == statId);
        }
    }
}

