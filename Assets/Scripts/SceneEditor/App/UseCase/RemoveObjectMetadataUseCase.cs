using SceneEditor.App.Command;
using SceneEditor.Domain;
using Shared.Domain;

namespace SceneEditor.App.UseCase
{
    public class RemoveObjectMetadataUseCase
    {
        private readonly CommandHistory _history;

        public RemoveObjectMetadataUseCase(CommandHistory history)
        {
            _history = history;
        }

        public void Execute(SceneObject obj, MetadataEntry entry)
        {
            if (obj == null || entry == null) return;
            _history.Record(new RemoveObjectMetadataCommand(obj, entry));
        }
    }
}
