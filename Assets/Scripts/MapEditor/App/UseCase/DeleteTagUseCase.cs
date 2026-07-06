using MapEditor.App.Command;
using MapEditor.Domain;
using SceneEditor.Domain;
using Shared.Domain;

namespace MapEditor.App.UseCase
{
    public class DeleteTagUseCase
    {
        private readonly CommandHistory _history;
        private readonly Map _map;

        public DeleteTagUseCase(CommandHistory history, Map map)
        {
            _history = history;
            _map = map;
        }

        public void Execute(MetadataEntry registryEntry)
        {
            if (registryEntry == null) return;
            _history.Record(new DeleteTagCommand(_map, registryEntry));
        }
    }
}
