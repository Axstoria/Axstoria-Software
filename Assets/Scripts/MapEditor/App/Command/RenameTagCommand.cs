using MapEditor.Domain;
using SceneEditor.Domain;
using Shared.Domain;

namespace MapEditor.App.Command
{
    public class RenameTagCommand : ICommand
    {
        public string Label => $"Edit tag {_newName}";

        private readonly Map _map;
        private readonly TagValue _tag;
        private readonly string _oldName, _newName;
        private readonly string _oldHexColor, _newHexColor;

        public RenameTagCommand(Map map, TagValue tag, string newName, string newHexColor)
        {
            _map = map;
            _tag = tag;
            _oldName = tag.Name;
            _oldHexColor = tag.HexColor;
            _newName = newName;
            _newHexColor = newHexColor;
        }

        public void Execute()
        {
            _tag.Name = _newName;
            _tag.HexColor = _newHexColor;
            _map.NotifyMetadataChanged();
        }

        public void Undo()
        {
            _tag.Name = _oldName;
            _tag.HexColor = _oldHexColor;
            _map.NotifyMetadataChanged();
        }

        public void Redo() => Execute();
    }
}
