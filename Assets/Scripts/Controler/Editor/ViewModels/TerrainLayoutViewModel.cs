using Domain;
using Loxodon.Framework.Observables;

namespace Controler.Editor.ViewModels
{
    public class TerrainLayoutViewModel : SceneViewModel
    {
        private readonly TerrainLayout _terrainLayout;
        public TerrainLayout Model => _terrainLayout;

        public TerrainLayoutViewModel(TerrainLayout terrainLayout) : base(terrainLayout)
        {
            _terrainLayout = terrainLayout;
        }
    }
}