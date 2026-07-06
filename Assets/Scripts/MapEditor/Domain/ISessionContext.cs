using System;

namespace MapEditor.Domain
{
    public interface ISessionContext
    {
        Player CurrentPlayer { get; }
        event Action OnCurrentPlayerChanged;
        void SetCurrentPlayer(string playerId);
    }
}
