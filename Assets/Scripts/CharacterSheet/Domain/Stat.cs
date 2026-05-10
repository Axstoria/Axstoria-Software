using System.Collections.Generic;
using UnityEngine;

namespace CharacterSheet.Domain
{
    public class StatValue
    {
        public string Id { get; }
        public float Value { get; set; }

        public StatValue(string statId, float initialValue = 0f)
        {
            Id = statId;
            Value = initialValue;
        }
    }
}

// Le bloc — un ensemble de valeurs de stats
/*public class StatBlock {
    public List<StatValue> Values { get; } = new();

    public StatValue Get(string statId) { ... }
    public void Set(string statId, float value) { ... }
}*/