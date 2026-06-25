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
}