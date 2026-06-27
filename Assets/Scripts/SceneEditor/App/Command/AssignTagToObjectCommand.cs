using SceneEditor.Domain;
using Shared.Domain;

namespace SceneEditor.App.Command
{
    public class AssignTagToObjectCommand : ICommand
    {
        private readonly SceneModel _obj;
        private readonly string     _tagId;
        private readonly bool       _assign;

        public string Label => (_assign ? "Assign" : "Remove") + " tag";

        public AssignTagToObjectCommand(SceneModel obj, string tagId, bool assign)
        {
            _obj    = obj;
            _tagId  = tagId;
            _assign = assign;
        }

        public void Execute() => Apply(_assign);
        public void Undo()    => Apply(!_assign);
        public void Redo()    => Apply(_assign);

        private void Apply(bool add)
        {
            if (add) _obj.Tags.Add(_tagId);
            else     _obj.Tags.Remove(_tagId);
        }
    }
}
