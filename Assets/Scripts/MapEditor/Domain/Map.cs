using System;
using System.Collections.Generic;
using SceneEditor.Domain;

namespace MapEditor.Domain
{
    public class Map
    {
        public string Id   { get; set; }
        public string Name { get; set; }

        public List<Token>         Tokens        { get; set; } = new();
        public List<Structure>     Structures    { get; set; } = new();
        public List<SceneObject>   Objects       { get; set; } = new();
        public List<MetadataEntry> Metadata      { get; set; } = new();
        public List<Player>        Players       { get; set; } = new();
        public TerrainLayout       TerrainLayout { get; set; }

        public event Action<SceneObject> OnObjectAdded;
        public event Action<SceneObject> OnObjectRemoved;
        public event EventHandler        OnMetadataChanged;
        public event Action<Player>      OnPlayerAdded;
        public event Action<Player>      OnPlayerRemoved;

        public void AddObject(SceneObject obj)
        {
            Objects.Add(obj);
            OnObjectAdded?.Invoke(obj);
        }

        public void RemoveObject(SceneObject obj)
        {
            Objects.Remove(obj);
            OnObjectRemoved?.Invoke(obj);
        }

        public void NotifyMetadataChanged() => OnMetadataChanged?.Invoke(this, EventArgs.Empty);

        public void AddPlayer(Player player)
        {
            Players.Add(player);
            OnPlayerAdded?.Invoke(player);
        }

        public void RemovePlayer(Player player)
        {
            Players.Remove(player);
            OnPlayerRemoved?.Invoke(player);
        }
    }
}
