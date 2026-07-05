using System;
using MapEditor.Domain;
using SceneEditor.Domain;
using Shared.Domain;

namespace MapEditor.App.Command
{
    public class CreateTagCommand : ICommand
    {
        public string Label => $"Create tag {(_entry.EntryValue as TagValue)?.Name}";

        private readonly Map _map;
        private readonly MetadataEntry _entry;

        public CreateTagCommand(Map map, string name, string hexColor)
        {
            _map = map;
            _entry = new MetadataEntry
            {
                EntryType = "tag",
                EntryValue = new TagValue { Id = Guid.NewGuid().ToString(), Name = name, HexColor = hexColor }
            };
        }

        public void Execute()
        {
            _map.Metadata.Add(_entry);
            _map.NotifyMetadataChanged();
        }

        public void Undo()
        {
            _map.Metadata.Remove(_entry);
            _map.NotifyMetadataChanged();
        }

        public void Redo() => Execute();
    }
}
