using UnityEngine;

namespace CharacterSheet.Domain
{
    public class WidgetStatBinding
    {
        public string StatId  { get; set; }
        
        public string DisplayNameOverride { get; set; }
        public bool ShowLabel { get; set; }
        public int Order { get; set; }
        
        public Color? ColorOverride { get; set; }
    }
}