using System.Linq;
using CharacterSheet.Domain;

namespace CharacterSheet.App.UseCase
{
    public class BindStatToWidgetUseCase
    {
        
        private readonly IStatDefinitionRepository _definitions;

        public BindStatToWidgetUseCase(IStatDefinitionRepository definitions)
        {
            _definitions = definitions;
        }
        
        public void Execute(SheetWidget widget, string statId)
        {
            if (!_definitions.Exists(statId)) return;
            if (widget == null) return;
            var stat = new WidgetStatBinding();
            stat.StatId = statId;
            widget.AddStat(stat);
        }
    } 
}