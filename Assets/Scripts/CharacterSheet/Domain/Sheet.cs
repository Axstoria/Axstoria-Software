using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Shared.Domain;

namespace CharacterSheet.Domain
{
    public class Sheet
    {
        public string Id { get; }
        public List<SheetWidget> Widgets { get; } = new();
        public List<Stat> Stats { get; } = new();
        
        public Stat GetStat(string statId) => Stats.FirstOrDefault(s => s.Id == statId);
        public bool HasStat(string statId) => Stats.Any(s => s.Id == statId);
        
        public bool HasBorder { get; set; }
        public float BorderThickness { get; set; }
        public Color BorderColor { get; set; }
        public Color BackgroundColor { get; set; }
        public string BackgroundImagePath { get; set; }
        
        public Sheet(string id)
        {
            Id = id;
        }
    }
}