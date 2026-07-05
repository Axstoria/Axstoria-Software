using MapEditor.App.Command;
using MapEditor.Domain;
using Shared.Domain;

namespace MapEditor.App.UseCase
{
    public class CreatePlayerUseCase
    {
        private readonly CommandHistory _history;
        private readonly Map _map;

        public CreatePlayerUseCase(CommandHistory history, Map map)
        {
            _history = history;
            _map = map;
        }

        public void Execute(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return;
            _history.Record(new CreatePlayerCommand(_map, name));
        }
    }
}
