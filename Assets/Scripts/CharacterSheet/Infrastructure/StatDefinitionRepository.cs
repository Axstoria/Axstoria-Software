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
            new Stat("force", "Force" , StatType.Int),
            new Stat("agility", "Agilité", StatType.Int),
            new Stat("intelligence", "Intelligence", StatType.Int),
            new Stat("social", "Social", StatType.Int),
            new Stat("intel", "Perception", StatType.Int),
            new Stat("wisdom", "Erudition", StatType.Int),
            new Stat("constitution", "Constitution", StatType.Int),
        };

        public StatDefinitionRepository()
        {
            var life = _definitions.FirstOrDefault(s => s.Id == "hp");
            life.AbsoluteMax = 8f;
        }

        public bool Exists(string statId)
            => _definitions.Any(s => s.Id == statId);

        public Stat Get(string statId)
            => _definitions.FirstOrDefault(s => s.Id == statId);
    }
}
