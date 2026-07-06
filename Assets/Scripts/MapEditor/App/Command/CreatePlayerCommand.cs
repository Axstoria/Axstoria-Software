using System;
using MapEditor.Domain;
using Shared.Domain;

namespace MapEditor.App.Command
{
    public class CreatePlayerCommand : ICommand
    {
        public string Label => $"Create player {_player.Name}";

        private readonly Map _map;
        private readonly Player _player;

        public CreatePlayerCommand(Map map, string name)
        {
            _map = map;
            _player = new Player
            {
                Id   = Guid.NewGuid().ToString(),
                Name = name
            };
        }

        public void Execute() => _map.AddPlayer(_player);
        public void Undo()    => _map.RemovePlayer(_player);
        public void Redo()    => Execute();
    }
}
