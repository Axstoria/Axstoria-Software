namespace Shared.Domain
{
    public enum StatType { Int, Float, Bool, Text}

    public class Stat
    {
        public string Id { get; }
        public string Name { get; }
        public StatType Type { get; }
        public float value { get; }
        public float Min { get; }
        public float Max { get; }
        
        public Stat(string id, string name) {
            Id = id;
            Name = name;
        }
    }
}