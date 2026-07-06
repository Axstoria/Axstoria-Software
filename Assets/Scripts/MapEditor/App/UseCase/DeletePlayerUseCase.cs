using MapEditor.App.Command;
using MapEditor.Domain;
using Shared.Domain;

namespace MapEditor.App.UseCase
{
    public class DeletePlayerUseCase
    {
        private readonly CommandHistory _history;
        private readonly Map _map;
        private readonly ISessionContext _session;

        public DeletePlayerUseCase(CommandHistory history, Map map, ISessionContext session)
        {
            _history = history;
            _map = map;
            _session = session;
        }

        public void Execute(Player player)
        {
            if (player == null || player.IsGameMaster) return;

            if (_session.CurrentPlayer?.Id == player.Id)
                _session.SetCurrentPlayer(null);

            _history.Record(new DeletePlayerCommand(_map, player));
        }
    }
}
