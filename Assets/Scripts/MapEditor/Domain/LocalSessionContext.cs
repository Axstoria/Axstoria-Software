using System;
using System.Linq;

namespace MapEditor.Domain
{
    public class LocalSessionContext : ISessionContext
    {
        private const string GameMasterId = "gm";

        private readonly Map _map;
        private readonly Player _gameMaster = new Player
        {
            Id           = GameMasterId,
            Name         = "Game Master",
            IsGameMaster = true
        };

        private string _currentPlayerId = GameMasterId;

        public event Action OnCurrentPlayerChanged;

        public LocalSessionContext(Map map)
        {
            _map = map;
        }

        public Player CurrentPlayer =>
            _currentPlayerId == GameMasterId
                ? _gameMaster
                : _map.Players.FirstOrDefault(p => p.Id == _currentPlayerId) ?? _gameMaster;

        public void SetCurrentPlayer(string playerId)
        {
            _currentPlayerId = string.IsNullOrEmpty(playerId) ? GameMasterId : playerId;
            OnCurrentPlayerChanged?.Invoke();
        }
    }
}
