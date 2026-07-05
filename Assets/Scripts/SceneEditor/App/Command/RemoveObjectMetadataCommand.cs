using System;
using System.Collections.Generic;
using SceneEditor.Domain;
using Shared.Domain;

namespace SceneEditor.App.Command
{
    public class RemoveObjectMetadataCommand : ICommand
    {
        public string Label => $"Remove metadata from {_obj.DisplayName}";

        private readonly SceneObject _obj;
        private readonly MetadataEntry _entry;
        private int _index = -1;

        public RemoveObjectMetadataCommand(SceneObject obj, MetadataEntry entry)
        {
            _obj = obj;
            _entry = entry;
        }

        public void Execute()
        {
            if (_obj.Metadata == null) return;
            _index = _obj.Metadata.IndexOf(_entry);
            if (_index < 0) return;
            _obj.Metadata.RemoveAt(_index);
            _obj.NotifyMetadataChanged();
        }

        public void Undo()
        {
            if (_index < 0) return;
            _obj.Metadata ??= new List<MetadataEntry>();
            int insertAt = Math.Min(_index, _obj.Metadata.Count);
            _obj.Metadata.Insert(insertAt, _entry);
            _obj.NotifyMetadataChanged();
        }

        public void Redo() => Execute();
    }
}
