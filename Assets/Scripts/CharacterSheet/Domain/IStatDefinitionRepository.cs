using Shared.Domain;
using UnityEngine;

namespace CharacterSheet.Domain
{
    public interface IStatDefinitionRepository
    {
        bool Exists(string statId);
        Stat Get(string statId);
    }
}
