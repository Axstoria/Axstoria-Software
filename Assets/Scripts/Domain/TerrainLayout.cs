namespace Domain
{
    /// <summary>
    /// Represents a terrain or height-map element that defines the geographical features of a map.
    /// Inherits from SceneModel and adds dimensional properties.
    /// </summary>
    /// <remarks>
    /// Terrain layouts are used to define walkable areas, hills, valleys, and other terrain features.
    /// Multiple terrain layouts can be combined to create complex, varied landscape structures.
    /// </remarks>
    public class TerrainLayout : SceneModel
    {
        /// <summary>
        /// Gets or sets the width of the terrain layout in game units.
        /// </summary>
        /// <remarks>
        /// Measured along the X-axis of the world.
        /// </remarks>
        public int Width { get; set; }
        
        /// <summary>
        /// Gets or sets the height of the terrain layout in game units.
        /// </summary>
        /// <remarks>
        /// Measured along the Z-axis (depth) of the world in a 3D context.
        /// </remarks>
        public int Height { get; set; }
    }
}