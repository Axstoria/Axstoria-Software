using System.Collections.Generic;
using Shared.Domain;

namespace GameSession.Domain
{
    public class Player : IHasTags
    {
        public string          Id           { get; set; }
        public string          Name         { get; set; }
        public bool            IsGameMaster { get; set; }
        public HashSet<string> Tags         { get; set; } = new();
    }
}
