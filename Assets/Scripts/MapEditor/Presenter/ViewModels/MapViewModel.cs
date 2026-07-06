using System;
using System.Collections.Generic;
using System.Linq;
using Loxodon.Framework.Observables;
using MapEditor.Domain;
using SceneEditor.Domain;
using SceneEditor.Presenter.ViewModels;

namespace MapEditor.Presenter.ViewModels
{
    public class MapViewModel : ObservableObject
    {
        private readonly Map _map;
        public Map Model => _map;

        public ObservableList<TokenViewModel>     Tokens     { get; } = new();
        public ObservableList<StructureViewModel> Structures { get; } = new();
        public ObservableList<ObjectViewModel>    Objects    { get; } = new();
        public ObservableList<TagViewModel>       Tags       { get; } = new();
        public ObservableList<PlayerViewModel>    Players    { get; } = new();

        public TerrainLayoutViewModel Terrain { get; private set; }

        private readonly Action<SceneObject> _onObjectAdded;
        private readonly Action<SceneObject> _onObjectRemoved;
        private readonly EventHandler        _onMetadataChanged;
        private readonly Action<Player>      _onPlayerAdded;
        private readonly Action<Player>      _onPlayerRemoved;

        public MapViewModel(Map map)
        {
            _map = map;

            foreach (var token in _map.Tokens)
                Tokens.Add(new TokenViewModel(token));

            foreach (var structure in _map.Structures)
                Structures.Add(new StructureViewModel(structure));

            foreach (var obj in _map.Objects)
                Objects.Add(new ObjectViewModel(obj));

            foreach (var player in _map.Players)
                Players.Add(new PlayerViewModel(player));

            SyncTags();

            if (_map.TerrainLayout != null)
                Terrain = new TerrainLayoutViewModel(_map.TerrainLayout);

            _onObjectAdded   = obj => Objects.Add(new ObjectViewModel(obj));
            _onObjectRemoved = obj =>
            {
                for (int i = 0; i < Objects.Count; i++)
                    if (Objects[i].Model == obj) { Objects.RemoveAt(i); return; }
            };
            _map.OnObjectAdded   += _onObjectAdded;
            _map.OnObjectRemoved += _onObjectRemoved;

            _onMetadataChanged = (_, __) => SyncTags();
            _map.OnMetadataChanged += _onMetadataChanged;

            _onPlayerAdded   = player => Players.Add(new PlayerViewModel(player));
            _onPlayerRemoved = player =>
            {
                for (int i = 0; i < Players.Count; i++)
                    if (Players[i].Model == player) { Players.RemoveAt(i); return; }
            };
            _map.OnPlayerAdded   += _onPlayerAdded;
            _map.OnPlayerRemoved += _onPlayerRemoved;
        }

        private void SyncTags()
        {
            var current = _map.Metadata
                .Where(e => e.EntryType == "tag" && e.EntryValue is TagValue)
                .ToList();

            for (int i = Tags.Count - 1; i >= 0; i--)
                if (!current.Contains(Tags[i].Entry))
                    Tags.RemoveAt(i);

            var existing = new HashSet<MetadataEntry>(Tags.Select(t => t.Entry));
            foreach (var entry in current)
                if (!existing.Contains(entry))
                    Tags.Add(new TagViewModel(entry));

            foreach (var tag in Tags)
                tag.Refresh();
        }

        public void Dispose()
        {
            _map.OnObjectAdded      -= _onObjectAdded;
            _map.OnObjectRemoved    -= _onObjectRemoved;
            _map.OnMetadataChanged  -= _onMetadataChanged;
            _map.OnPlayerAdded      -= _onPlayerAdded;
            _map.OnPlayerRemoved    -= _onPlayerRemoved;
        }
    }
}
