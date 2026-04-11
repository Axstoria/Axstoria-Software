using Domain;

namespace App.Commands
{
    /// <summary>
    /// Command to add a new SceneObject to the map.
    /// </summary>
    public class AddObjectCommand : ICommand
    {
        public string Label => $"Add {_obj.DisplayName}";

        private readonly Map         _map;
        private readonly SceneObject _obj;

        public AddObjectCommand(Map map, SceneObject obj)
        {
            _map = map;
            _obj = obj;
        }

        public void Execute() => _map.AddObject(_obj);
        public void Undo()    => _map.RemoveObject(_obj);
        public void Redo()    => Execute();
    }
}
