namespace Domain.Character
{
    public abstract class Stat
    {
        public string Id { get; }
        public string DisplayName { get; set; }
        
        protected Stat(string id, string displayName)
        {
            Id = id;
            DisplayName = displayName;
        }
    }
}