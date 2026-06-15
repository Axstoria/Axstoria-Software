using System.Collections.Generic;

namespace SceneEditor.Domain
{
    public class MetadataEntry
    {
        public string EntryType { get; set; }
        public string EntryValue { get; set; }
    }

    public class SceneObject : SceneModel
    {
        public bool   IsInteractable { get; set; }
        public string DisplayName    { get; set; }
        public string Category       { get; set; }
        public bool   IsImported     { get; set; }
        public string ImportPath     { get; set; }
        public List<MetadataEntry> Metadata { get; set; }
    }
}
