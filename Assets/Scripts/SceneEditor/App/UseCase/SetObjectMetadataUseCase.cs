using SceneEditor.App.Command;
using SceneEditor.Domain;
using Shared.Domain;

namespace SceneEditor.App.UseCase
{
    public class SetObjectMetadataUseCase
    {
        private readonly CommandHistory _history;

        public SetObjectMetadataUseCase(CommandHistory history)
        {
            _history = history;
        }

        public void Execute(SceneObject obj, string metadataType, MetadataValue metadataValue)
        {
            if (obj == null) return;
            if (string.IsNullOrWhiteSpace(metadataType)) return;

            _history.Record(new SetObjectMetadataCommand(obj, metadataType, metadataValue));
        }
    }
}