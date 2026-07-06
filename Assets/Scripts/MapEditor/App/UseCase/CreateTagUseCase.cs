using MapEditor.App.Command;
using MapEditor.Domain;
using Shared.Domain;

namespace MapEditor.App.UseCase
{
    public class CreateTagUseCase
    {
        private readonly CommandHistory _history;
        private readonly Map _map;

        public CreateTagUseCase(CommandHistory history, Map map)
        {
            _history = history;
            _map = map;
        }

        public void Execute(string name, string hexColor)
        {
            if (string.IsNullOrWhiteSpace(name)) return;
            _history.Record(new CreateTagCommand(_map, name, hexColor));
        }
    }
}
