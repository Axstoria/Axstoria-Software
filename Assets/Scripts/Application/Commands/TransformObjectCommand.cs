using Domain;

namespace App.Commands
{
    /// <summary>
    /// Command to transform a SceneObject by changing its position, rotation, or scale.
    /// </summary>
    public class TransformObjectCommand : ICommand
    {
        public string Label { get; }

        private readonly SceneObject _obj;
        private readonly TransformModel _before;
        private readonly TransformModel _after;

        public TransformObjectCommand(SceneObject obj, TransformModel before, TransformModel after, string label)
        {
            _obj    = obj;
            _before = before;
            _after  = after;
            Label   = label;
        }

        public void Execute() => _obj.Transform = _after;
        public void Undo()    => _obj.Transform = _before;
        public void Redo()    => Execute();
    }
}
