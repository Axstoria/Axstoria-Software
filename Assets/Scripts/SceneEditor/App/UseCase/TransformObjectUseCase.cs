using SceneEditor.App.Command;
using SceneEditor.Domain;
using Shared.Domain;

namespace SceneEditor.App.UseCase
{
    public class TransformObjectUseCase
    {
        private readonly CommandHistory _history;

        public TransformObjectUseCase(CommandHistory history)
        {
            _history = history;
        }

        public void Execute(SceneObject obj, TransformModel after, string label)
        {
            var before = obj.Transform;
            _history.Record(new TransformObjectCommand(obj, before, after, label));
        }
    }
}
