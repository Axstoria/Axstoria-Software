// using Domain;
// using Loxodon.Framework.Observables;

// namespace Controler.Editor.ViewModels
// {
//     /// <summary>
//     /// View model for managing hex-based terrain layouts.
//     /// Provides observable collection for hex tiles enabling UI synchronization.
//     /// </summary>
//     public class HexTerrainLayoutViewModel : TerrainLayoutViewModel
//     {
//         private readonly HexTerrainLayout _hexTerrainLayout;
        
//         /// <summary>
//         /// Gets the observable collection of hex tile view models.
//         /// </summary>
//         public ObservableList<HexTileViewModel> Tiles { get; }
        
//         /// <summary>
//         /// Gets the underlying HexTerrainLayout model.
//         /// </summary>
//         public new HexTerrainLayout Model => _hexTerrainLayout;

//         public HexTerrainLayoutViewModel(HexTerrainLayout hexTerrainLayout) : base(hexTerrainLayout)
//         {
//             _hexTerrainLayout = hexTerrainLayout;
//             Tiles = new ObservableList<HexTileViewModel>();
            
//             foreach (var tile in _hexTerrainLayout.Tiles)
//             {
//                 Tiles.Add(new HexTileViewModel(tile));
//             }
//         }

//         /// <summary>
//         /// Adds a new hex tile to the terrain and synchronizes with the view model.
//         /// </summary>
//         public void AddTile(HexTile tile)
//         {
//             _hexTerrainLayout.Tiles.Add(tile);
//             Tiles.Add(new HexTileViewModel(tile));
//         }

//         /// <summary>
//         /// Removes a hex tile from the terrain and synchronizes with the view model.
//         /// </summary>
//         public void RemoveTile(HexTileViewModel tileViewModel)
//         {
//             _hexTerrainLayout.Tiles.Remove(tileViewModel.Model);
//             Tiles.Remove(tileViewModel);
//         }
        
//         /// <summary>
//         /// Clears all tiles from the terrain.
//         /// </summary>
//         public void ClearAllTiles()
//         {
//             _hexTerrainLayout.Tiles.Clear();
//             Tiles.Clear();
//         }
//     }
// }