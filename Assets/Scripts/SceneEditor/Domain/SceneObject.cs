using System;
using System.Collections.Generic;

namespace SceneEditor.Domain
{
    public abstract class MetadataValue { }

    public class NoteValue : MetadataValue
    {
        public string Text { get; set; }
    }

    public class TagValue : MetadataValue
    {
        public string Id       { get; set; }
        public string Name     { get; set; }
        public string HexColor { get; set; } = "#FFFFFF";
    }

    public class SheetValue : MetadataValue
    {
        // TODO: Implement this class when sheets are done
    }

    public class MetadataEntry
    {
        public string EntryType { get; set; }
        public MetadataValue EntryValue { get; set; }
    }

    public class SceneObject : SceneModel
    {
        public bool   IsInteractable { get; set; }
        public string DisplayName    { get; set; }
        public string Category       { get; set; }
        public bool   IsImported     { get; set; }
        public string ImportPath     { get; set; }
        public List<MetadataEntry> Metadata { get; set; }
        public event EventHandler OnMetadataChanged;
        public void NotifyMetadataChanged() => OnMetadataChanged?.Invoke(this, EventArgs.Empty);
    }
}
