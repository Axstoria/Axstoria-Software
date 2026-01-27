using Domain;
using Loxodon.Framework.Observables;
using UnityEngine;

namespace Controler.Editor.ViewModels
{
    /// <summary>
    /// View model for a single hex tile.
    /// Provides observable properties for tile placement data.
    /// </summary>
    public class HexTileViewModel : ObservableObject
    {
        private readonly HexTile _tile;
        
        public HexTile Model => _tile;
        
        public ObservableProperty<int> PrefabIndex { get; }
        public ObservableProperty<int> X { get; }
        public ObservableProperty<int> Y { get; }
        public ObservableProperty<int> Z { get; }
        public ObservableProperty<float> YRotation { get; }

        public HexTileViewModel(HexTile tile)
        {
            _tile = tile;
            PrefabIndex = new ObservableProperty<int>(tile.PrefabIndex);
            X = new ObservableProperty<int>(tile.X);
            Y = new ObservableProperty<int>(tile.Y);
            Z = new ObservableProperty<int>(tile.Z);
            YRotation = new ObservableProperty<float>(tile.YRotation);
            
            // Sync changes back to model
            PrefabIndex.ValueChanged += (_, __) => _tile.PrefabIndex = PrefabIndex.Value;
            X.ValueChanged += (_, __) => _tile.X = X.Value;
            Y.ValueChanged += (_, __) => _tile.Y = Y.Value;
            Z.ValueChanged += (_, __) => _tile.Z = Z.Value;
            YRotation.ValueChanged += (_, __) => _tile.YRotation = YRotation.Value;
        }
        
        public Vector3Int GetCell()
        {
            return new Vector3Int(_tile.X, _tile.Y, _tile.Z);
        }
    }
}