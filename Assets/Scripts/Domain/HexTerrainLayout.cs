// using System.Collections.Generic;

// namespace Domain
// {
//     /// <summary>
//     /// Represents the hex-based terrain layout for the map.
//     /// Contains all placed tiles that make up the hex grid.
//     /// </summary>
//     public class HexTerrainLayout : TerrainLayout
//     {
//         /// <summary>
//         /// Gets or sets the collection of hex tiles placed on this terrain.
//         /// </summary>
//         public List<HexTile> Tiles { get; set; } = new List<HexTile>();
//     }
    
//     /// <summary>
//     /// Represents a single hex tile in the terrain layout.
//     /// </summary>
//     public class HexTile
//     {
//         /// <summary>
//         /// Gets or sets the prefab index used to instantiate this tile.
//         /// </summary>
//         public int PrefabIndex { get; set; }
        
//         /// <summary>
//         /// Gets or sets the X coordinate in the hex grid.
//         /// </summary>
//         public int X { get; set; }
        
//         /// <summary>
//         /// Gets or sets the Y coordinate in the hex grid.
//         /// </summary>
//         public int Y { get; set; }
        
//         /// <summary>
//         /// Gets or sets the Z coordinate in the hex grid.
//         /// </summary>
//         public int Z { get; set; }
        
//         /// <summary>
//         /// Gets or sets the Y-axis rotation of the tile in degrees.
//         /// </summary>
//         public float YRotation { get; set; }
//     }
// }