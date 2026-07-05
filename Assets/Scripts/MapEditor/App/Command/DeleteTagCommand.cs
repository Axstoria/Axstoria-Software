using System;
using System.Collections.Generic;
using MapEditor.Domain;
using SceneEditor.Domain;
using Shared.Domain;

namespace MapEditor.App.Command
{
    public class DeleteTagCommand : ICommand
    {
        public string Label => $"Delete tag {_tagName}";

        private readonly Map _map;
        private readonly MetadataEntry _registryEntry;
        private readonly string _tagId;
        private readonly string _tagName;
        private readonly List<(SceneObject obj, MetadataEntry entry, int index)> _removedAssignments = new();

        public DeleteTagCommand(Map map, MetadataEntry registryEntry)
        {
            _map = map;
            _registryEntry = registryEntry;
            var tag = (TagValue)registryEntry.EntryValue;
            _tagId = tag.Id;
            _tagName = tag.Name;
        }

        public void Execute()
        {
            _map.Metadata.Remove(_registryEntry);

            _removedAssignments.Clear();
            foreach (var obj in _map.Objects)
            {
                if (obj.Metadata == null) continue;
                for (int i = obj.Metadata.Count - 1; i >= 0; i--)
                {
                    if (obj.Metadata[i].EntryValue is TagValue tv && tv.Id == _tagId)
                    {
                        _removedAssignments.Add((obj, obj.Metadata[i], i));
                        obj.Metadata.RemoveAt(i);
                    }
                }
            }

            _map.NotifyMetadataChanged();
            foreach (var (obj, _, _) in _removedAssignments)
                obj.NotifyMetadataChanged();
        }

        public void Undo()
        {
            _map.Metadata.Add(_registryEntry);

            foreach (var (obj, entry, index) in _removedAssignments)
            {
                int insertAt = Math.Min(index, obj.Metadata.Count);
                obj.Metadata.Insert(insertAt, entry);
            }

            _map.NotifyMetadataChanged();
            foreach (var (obj, _, _) in _removedAssignments)
                obj.NotifyMetadataChanged();

            _removedAssignments.Clear();
        }

        public void Redo() => Execute();
    }
}
