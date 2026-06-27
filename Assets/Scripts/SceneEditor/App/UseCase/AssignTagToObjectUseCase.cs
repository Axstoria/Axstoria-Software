using SceneEditor.App.Command;
using SceneEditor.Domain;
using Shared.Domain;

namespace SceneEditor.App.UseCase
{
    public class AssignTagToObjectUseCase
    {
        private readonly CommandHistory _history;

        public AssignTagToObjectUseCase(CommandHistory history)
        {
            _history = history;
        }

        public void Execute(SceneModel obj, string tagId, bool assign)
        {
            _history.Record(new AssignTagToObjectCommand(obj, tagId, assign));
        }
    }
}
