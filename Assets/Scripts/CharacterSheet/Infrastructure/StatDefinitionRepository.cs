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
            new Stat("hp", "Points de vie", StatType.Int),
            new Stat("attack", "Attaque" , StatType.Int),
            new Stat("defense", "Défense", StatType.Int)
        };

        public StatDefinitionRepository()
        {
            var life = _definitions.FirstOrDefault(s => s.Id == "hp");
            life.AbsoluteMax = 100f;
        }

        public bool Exists(string statId)
            => _definitions.Any(s => s.Id == statId);

        public Stat Get(string statId)
            => _definitions.FirstOrDefault(s => s.Id == statId);
    }
}
