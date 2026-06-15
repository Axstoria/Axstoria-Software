using Shared.Domain;
using SceneEditor.Domain;
using System.Collections.Generic;

namespace SceneEditor.App.Command
{
    public class SetObjectMetadataCommand : ICommand
    {
        public string Label => $"Add metadata to {_obj.DisplayName}";
        private readonly SceneObject _obj;
        private readonly MetadataEntry _newEntry;
        private readonly List<MetadataEntry> _previousMetadata;

        public SetObjectMetadataCommand(SceneObject obj, string metadataType, string metadataValue)
        {
            _obj = obj;
            _newEntry = new MetadataEntry
            {
                EntryType = metadataType,
                EntryValue = metadataValue
            };
            _previousMetadata = new List<MetadataEntry>(_obj.Metadata ?? new List<MetadataEntry>());
        }

        public void Execute()
        {
            _obj.Metadata ??= new List<MetadataEntry>();
            _obj.Metadata.Add(_newEntry);
        }

        public void Undo()
        {
            _obj.Metadata = new List<MetadataEntry>(_previousMetadata);
        }

        public void Redo() => Execute();
    }
}