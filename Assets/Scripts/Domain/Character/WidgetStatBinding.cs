namespace Domain.Character
{
    public class WidgetStatBinding
    {
        public string StatId  { get; }
        
        public string DisplayNameOverride { get; set; }
        public bool ShowLabel { get; set; }
        public int Order { get; set; }
        
        public WidgetStatBinding(string StatId)
        {
            this.StatId = StatId;
        }
    }
}