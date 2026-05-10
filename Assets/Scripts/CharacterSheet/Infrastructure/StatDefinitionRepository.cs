using System.Collections.Generic;
using System.Linq;
using CharacterSheet.Domain;
using Shared.Domain;

namespace CharacterSheet.Infrastructure
{
    public class StatDefinitionRepository : IStatDefinitionRepository
    {
        private static readonly List<Stat> _definitions = new()
        {
            new Stat("hp", "Points de vie"),
            new Stat("attack", "Attaque"),
            new Stat("defense", "Défense")
        };

        public bool Exists(string statId)
            => _definitions.Any(s => s.Id == statId);

        public Stat Get(string statId)
            => _definitions.FirstOrDefault(s => s.Id == statId);
    }
}
