using Shared.Domain;
using SceneEditor.Domain;
using System.Collections.Generic;

namespace SceneEditor.App.Command
{
    public class SetObjectMetadataCommand : ICommand
    {
        public string Label => "Add metadata";
        private readonly IHasMetadata _obj;
        private readonly string _type;
        private readonly MetadataValue _value;
        private MetadataEntry _entry;

        public SetObjectMetadataCommand(IHasMetadata obj, string metadataType, MetadataValue metadataValue)
        {
            _obj = obj;
            _type = metadataType;
            _value = metadataValue;
        }

        public void Execute()
        {
            _obj.Metadata ??= new List<MetadataEntry>();
            _entry = new MetadataEntry { EntryType = _type, EntryValue = _value };
            _obj.Metadata.Add(_entry);
            _obj.NotifyMetadataChanged();
        }

        public void Undo()
        {
            if (_obj.Metadata == null || _entry == null)
                return;
            _obj.Metadata.Remove(_entry);
            _obj.NotifyMetadataChanged();
        }

        public void Redo() => Execute();
    }
}