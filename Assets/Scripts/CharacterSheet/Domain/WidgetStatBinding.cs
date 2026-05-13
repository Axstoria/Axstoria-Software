using UnityEngine;

namespace CharacterSheet.Domain
{
    public class WidgetStatBinding
    {
        public string StatId  { get; }
        
        public string DisplayNameOverride { get; set; }
        public bool ShowLabel { get; set; }
        public int Order { get; set; }
        
        public Color? ColorOverride { get; set; }
        
        public WidgetStatBinding(string statId)
        {
            this.StatId = statId;
        }
    }
}