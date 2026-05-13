using CharacterSheet.Domain;

namespace CharacterSheet.App.UseCase
{
    public class AddStatUseCase
    {
        private readonly IStatDefinitionRepository _definitions;
        
        public void Execute(Sheet sheet, string statId,  float initialValue = 0f)
        {
            if (!_definitions.Exists(statId)) return;
            if (sheet.HasStat(statId)) return;
            sheet.AddStat(new StatValue(statId,  initialValue));
        }
    }
}
