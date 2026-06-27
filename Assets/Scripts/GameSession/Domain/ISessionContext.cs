using System.Collections.Generic;

namespace GameSession.Domain
{
    public interface ISessionContext
    {
        Player       CurrentPlayer { get; }
        List<Player> Players       { get; }
    }
}
