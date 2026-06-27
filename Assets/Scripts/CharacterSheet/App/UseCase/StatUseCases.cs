using CharacterSheet.Domain;
using Shared.Domain;

namespace CharacterSheet.App.UseCase
{
    public class GetStatUseCase
    {
        private readonly IStatDefinitionRepository _statRepository;

        public GetStatUseCase(IStatDefinitionRepository statRepository)
        {
            _statRepository = statRepository;
        }

        public Stat Execute(string statId)
        {
            return _statRepository.Get(statId);
        }
    }
    
    public class AddStatUseCase
    {
        private readonly IStatDefinitionRepository _definitions;

        public AddStatUseCase(IStatDefinitionRepository definitions)
        {
            _definitions = definitions;
        }
        
        public void Execute(Sheet sheet, string statId,  float initialValue = 0f)
        {
            if (!_definitions.Exists(statId)) return;
            if (sheet.HasStat(statId)) return;
            sheet.AddStat(new StatValue(statId,  initialValue));
        }
    }
    
    public class RemoveStatUseCase
    {
        public void Execute(Sheet sheet, string statId)
        {
            if (!sheet.HasStat(statId)) return;
            sheet.RemoveStat(statId);
        }
    }
    
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
            var stat = new WidgetStatBinding
            {
                StatId = statId
            };
            widget.AddStat(stat);
        }
    }
    
    public class UnbindStatUseCase
    {
        public void Execute(SheetWidget widget, string statId)
        {
            widget?.RemoveStat(statId);
        }
    }
    
    public class UpdateStatValueUseCase
    {
        public void Execute(Sheet sheet, string statId)
        {
            return;
        }
    }
}