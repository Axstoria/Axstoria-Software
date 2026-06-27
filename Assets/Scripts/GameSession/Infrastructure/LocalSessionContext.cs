using System.Collections.Generic;
using GameSession.Domain;

namespace GameSession.Infrastructure
{
    public class LocalSessionContext : ISessionContext
    {
        public Player       CurrentPlayer { get; private set; }
        public List<Player> Players       { get; } = new();

        public LocalSessionContext()
        {
            var gm = new Player
            {
                Id           = "gm",
                Name         = "Game Master",
                IsGameMaster = true
            };
            Players.Add(gm);
            CurrentPlayer = gm;
        }

        public void SetCurrentPlayer(string playerId)
        {
            foreach (var p in Players)
            {
                if (p.Id != playerId) continue;
                CurrentPlayer = p;
                return;
            }
        }

        public void AddPlayer(Player player)
        {
            Players.Add(player);
        }
    }
}
