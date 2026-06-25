namespace Shared.Domain
{
    public enum StatType { Int, Float, Bool, Text}

    public class Stat
    {
        public string Id { get; }
        public string Name { get; }
        public StatType Type { get; }
        public float AbsoluteMin { get; set; }
        public float AbsoluteMax { get; set; }
        
        public Stat(string id, string name, StatType type, float min = 0, float max = 100) {
            Id = id;
            Name = name;
            Type = type;
            AbsoluteMin = min;
            AbsoluteMax = max;
        }
    }
}