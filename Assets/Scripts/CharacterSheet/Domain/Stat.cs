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