using System;
using System.Collections.Generic;
using SceneEditor.Domain;

namespace MapEditor.Domain
{
    public class Player : IHasMetadata
    {
        public string Id           { get; set; }
        public string Name         { get; set; }
        public bool   IsGameMaster { get; set; }
        public string PawnId       { get; set; }

        public List<MetadataEntry> Metadata { get; set; } = new();
        public event EventHandler OnMetadataChanged;
        public void NotifyMetadataChanged() => OnMetadataChanged?.Invoke(this, EventArgs.Empty);
    }
}
