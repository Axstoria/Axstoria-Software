using SceneEditor.Domain;
using DomainGrid = Grid.Domain.Grid;

namespace MapEditor.Domain
{
    public class TerrainLayout : SceneModel
    {
        public int     Width     { get; set; }
        public int     Depth     { get; set; }
        public int     Thickness { get; set; }
        public float   Height    { get; set; }
        public float[] Color     { get; set; }

        public DomainGrid Grid { get; set; }
    }
}
