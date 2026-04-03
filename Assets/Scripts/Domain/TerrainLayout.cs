namespace Domain
{
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
