using MapEditor.App.Command;
using MapEditor.Domain;
using SceneEditor.Domain;
using Shared.Domain;

namespace MapEditor.App.UseCase
{
    public class RenameTagUseCase
    {
        private readonly CommandHistory _history;
        private readonly Map _map;

        public RenameTagUseCase(CommandHistory history, Map map)
        {
            _history = history;
            _map = map;
        }

        public void Execute(TagValue tag, string newName, string newHexColor)
        {
            if (tag == null || string.IsNullOrWhiteSpace(newName)) return;
            _history.Record(new RenameTagCommand(_map, tag, newName, newHexColor));
        }
    }
}
