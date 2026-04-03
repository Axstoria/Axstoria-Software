using Domain;
using Loxodon.Framework.Observables;

namespace Controler.Editor.ViewModels
{
    public class MapViewModel : ObservableObject
    {
        private readonly Map _map;
        public Map Model => _map;

        public ObservableList<TokenViewModel>     Tokens     { get; } = new();
        public ObservableList<StructureViewModel> Structures { get; } = new();
        public ObservableList<ObjectViewModel>    Objects    { get; } = new();

        public TerrainLayoutViewModel Terrain { get; private set; }

        public MapViewModel(Map map)
        {
            _map = map;

            foreach (var token in _map.Tokens)
                Tokens.Add(new TokenViewModel(token));

            foreach (var structure in _map.Structures)
                Structures.Add(new StructureViewModel(structure));

            foreach (var obj in _map.Objects)
                Objects.Add(new ObjectViewModel(obj));

            if (_map.TerrainLayout != null)
                Terrain = new TerrainLayoutViewModel(_map.TerrainLayout);

            _map.OnObjectAdded   += obj => Objects.Add(new ObjectViewModel(obj));
            _map.OnObjectRemoved += obj =>
            {
                for (int i = 0; i < Objects.Count; i++)
                {
                    if (Objects[i].Model == obj) { Objects.RemoveAt(i); return; }
                }
            };
        }
    }
}
