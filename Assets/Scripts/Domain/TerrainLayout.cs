namespace Domain
{
    /// <summary>
    /// Represents the terrain layout of the map, including dimensions, height, color, and grid information.
    /// </summary>
    public class TerrainLayout : SceneModel
    {
        public int     Width      { get; set; }
        public int     Depth      { get; set; }
        public int     Thickness  { get; set; }
        public float   Height     { get; set; }
        public float[] Color      { get; set; }

        public Grid Grid { get; set; }
    }
}
