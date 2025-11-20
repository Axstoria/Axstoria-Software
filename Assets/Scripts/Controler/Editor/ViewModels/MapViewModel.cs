using Map = Domain.Map;
using Loxodon.Framework.Observables;
using Domain;

namespace Controler.Editor.ViewModels
{
    public class MapViewModel : ObservableObject
    {
        private Map _map;
        public ObservableList<TokenViewModel> Tokens { get; }
        public ObservableList<StructureViewModel> Structures { get; }
        public ObservableList<ObjectViewModel> Objects { get; }
        public ObservableList<TerrainLayoutViewModel> TerrainLayouts { get; }

        public MapViewModel(Map map)
        {
            _map = map;
            Tokens = new ObservableList<TokenViewModel>();
            Structures = new ObservableList<StructureViewModel>();
            Objects = new ObservableList<ObjectViewModel>();
            TerrainLayouts = new ObservableList<TerrainLayoutViewModel>();
            
            foreach(var token in _map.Tokens)
                Tokens.Add(new TokenViewModel(token));
            
            foreach(var structure in _map.Structures)
                Structures.Add(new StructureViewModel(structure));
            
            foreach(var obj in _map.Objects)
                Objects.Add(new ObjectViewModel(obj));
            
            foreach(var terrain in _map.TerrainLayouts)
                TerrainLayouts.Add(new TerrainLayoutViewModel(terrain));
        }

        public void AddToken(Token token)
        {
            _map.Tokens.Add(token);
            Tokens.Add(new TokenViewModel(token));
        }

        public void RemoveToken(TokenViewModel tokenViewModel)
        {
            _map.Tokens.Remove(tokenViewModel.Model);
            Tokens.Remove(tokenViewModel);
        }

        public void AddStructure(Structure structure)
        {
            _map.Structures.Add(structure);
            Structures.Add(new StructureViewModel(structure));
        }

        public void RemoveStructure(StructureViewModel structureViewModel)
        {
            _map.Structures.Remove(structureViewModel.Model);
            Structures.Remove(structureViewModel);
        }

        public void AddObject(SceneObject obj)
        {
            _map.Objects.Add(obj);
            Objects.Add(new ObjectViewModel(obj));
        }

        public void RemoveObject(ObjectViewModel objectViewModel)
        {
            _map.Objects.Remove(objectViewModel.Model);
            Objects.Remove(objectViewModel);
        }

        public void AddTerrainLayout(TerrainLayout layout)
        {
            _map.TerrainLayouts.Add(layout);
            TerrainLayouts.Add(new TerrainLayoutViewModel(layout));
        }

        public void RemoveTerrainLayout(TerrainLayoutViewModel layoutViewModel)
        {
            _map.TerrainLayouts.Remove(layoutViewModel.Model);
            TerrainLayouts.Remove(layoutViewModel);
        }
    }
}