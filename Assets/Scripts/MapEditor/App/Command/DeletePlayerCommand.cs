using MapEditor.Domain;
using Shared.Domain;

namespace MapEditor.App.Command
{
    public class DeletePlayerCommand : ICommand
    {
        public string Label => $"Delete player {_player.Name}";

        private readonly Map _map;
        private readonly Player _player;

        public DeletePlayerCommand(Map map, Player player)
        {
            _map = map;
            _player = player;
        }

        public void Execute() => _map.RemovePlayer(_player);
        public void Undo()    => _map.AddPlayer(_player);
        public void Redo()    => Execute();
    }
}
