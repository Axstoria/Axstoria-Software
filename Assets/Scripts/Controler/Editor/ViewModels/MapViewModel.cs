using Map = Domain.Map;
using Loxodon.Framework.Observables;
using Domain;

namespace Controler.Editor.ViewModels
{
    /// <summary>
    /// View model for managing all entities within a game map.
    /// Provides observable collections for tokens, structures, objects, and terrain layouts,
    /// enabling the UI to display and edit map contents with automatic synchronization.
    /// </summary>
    /// <remarks>
    /// This is the primary view model for the map editor interface.
    /// It uses ObservableList collections to ensure that UI elements automatically update
    /// when entities are added or removed from the map.
    /// The MapViewModel maintains bidirectional synchronization between the underlying Map model
    /// and the view layer through wrapped view models.
    /// </remarks>
    public class MapViewModel : ObservableObject
    {
        /// <summary>
        /// The underlying Map model that this view model represents.
        /// </summary>
        private Map _map;
        
        /// <summary>
        /// Gets the observable collection of token view models on the map.
        /// </summary>
        /// <remarks>
        /// This collection automatically notifies the UI when tokens are added or removed.
        /// Each TokenViewModel wraps a Token model and provides observable properties for binding.
        /// </remarks>
        public ObservableList<TokenViewModel> Tokens { get; }
        
        /// <summary>
        /// Gets the observable collection of structure view models on the map.
        /// </summary>
        /// <remarks>
        /// This collection automatically notifies the UI when structures are added or removed.
        /// Each StructureViewModel wraps a Structure model and provides observable properties for binding.
        /// </remarks>
        public ObservableList<StructureViewModel> Structures { get; }
        
        /// <summary>
        /// Gets the observable collection of interactive scene object view models on the map.
        /// </summary>
        /// <remarks>
        /// This collection automatically notifies the UI when objects are added or removed.
        /// Each ObjectViewModel wraps a SceneObject model and provides observable properties for binding.
        /// </remarks>
        public ObservableList<ObjectViewModel> Objects { get; }
        
        /// <summary>
        /// Gets the observable collection of terrain layout view models on the map.
        /// </summary>
        /// <remarks>
        /// This collection automatically notifies the UI when terrain layouts are added or removed.
        /// Each TerrainLayoutViewModel wraps a TerrainLayout model and provides observable properties for binding.
        /// </remarks>
        public ObservableList<TerrainLayoutViewModel> TerrainLayouts { get; }

        /// <summary>
        /// Initializes a new instance of the MapViewModel class.
        /// </summary>
        /// <param name="map">The Map model to be wrapped by this view model.</param>
        /// <remarks>
        /// This constructor initializes all observable collections and populates them with view models
        /// wrapping the entities in the underlying map model.
        /// </remarks>
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

        /// <summary>
        /// Adds a new token to the map and synchronizes it with the view model.
        /// </summary>
        /// <param name="token">The Token model to add to the map.</param>
        /// <remarks>
        /// This method adds the token to both the underlying map and the observable collection,
        /// ensuring the UI is automatically updated.
        /// </remarks>
        public void AddToken(Token token)
        {
            _map.Tokens.Add(token);
            Tokens.Add(new TokenViewModel(token));
        }

        /// <summary>
        /// Removes a token from the map and synchronizes it with the view model.
        /// </summary>
        /// <param name="tokenViewModel">The TokenViewModel to remove from the map.</param>
        /// <remarks>
        /// This method removes the token from both the underlying map and the observable collection,
        /// ensuring the UI is automatically updated.
        /// </remarks>
        public void RemoveToken(TokenViewModel tokenViewModel)
        {
            _map.Tokens.Remove(tokenViewModel.Model);
            Tokens.Remove(tokenViewModel);
        }

        /// <summary>
        /// Adds a new structure to the map and synchronizes it with the view model.
        /// </summary>
        /// <param name="structure">The Structure model to add to the map.</param>
        /// <remarks>
        /// This method adds the structure to both the underlying map and the observable collection,
        /// ensuring the UI is automatically updated.
        /// </remarks>
        public void AddStructure(Structure structure)
        {
            _map.Structures.Add(structure);
            Structures.Add(new StructureViewModel(structure));
        }

        /// <summary>
        /// Removes a structure from the map and synchronizes it with the view model.
        /// </summary>
        /// <param name="structureViewModel">The StructureViewModel to remove from the map.</param>
        /// <remarks>
        /// This method removes the structure from both the underlying map and the observable collection,
        /// ensuring the UI is automatically updated.
        /// </remarks>
        public void RemoveStructure(StructureViewModel structureViewModel)
        {
            _map.Structures.Remove(structureViewModel.Model);
            Structures.Remove(structureViewModel);
        }

        /// <summary>
        /// Adds a new interactive scene object to the map and synchronizes it with the view model.
        /// </summary>
        /// <param name="obj">The SceneObject model to add to the map.</param>
        /// <remarks>
        /// This method adds the object to both the underlying map and the observable collection,
        /// ensuring the UI is automatically updated.
        /// </remarks>
        public void AddObject(SceneObject obj)
        {
            _map.Objects.Add(obj);
            Objects.Add(new ObjectViewModel(obj));
        }

        /// <summary>
        /// Removes an interactive scene object from the map and synchronizes it with the view model.
        /// </summary>
        /// <param name="objectViewModel">The ObjectViewModel to remove from the map.</param>
        /// <remarks>
        /// This method removes the object from both the underlying map and the observable collection,
        /// ensuring the UI is automatically updated.
        /// </remarks>
        public void RemoveObject(ObjectViewModel objectViewModel)
        {
            _map.Objects.Remove(objectViewModel.Model);
            Objects.Remove(objectViewModel);
        }

        /// <summary>
        /// Adds a new terrain layout to the map and synchronizes it with the view model.
        /// </summary>
        /// <param name="layout">The TerrainLayout model to add to the map.</param>
        /// <remarks>
        /// This method adds the terrain layout to both the underlying map and the observable collection,
        /// ensuring the UI is automatically updated.
        /// </remarks>
        public void AddTerrainLayout(TerrainLayout layout)
        {
            _map.TerrainLayouts.Add(layout);
            TerrainLayouts.Add(new TerrainLayoutViewModel(layout));
        }

        /// <summary>
        /// Removes a terrain layout from the map and synchronizes it with the view model.
        /// </summary>
        /// <param name="layoutViewModel">The TerrainLayoutViewModel to remove from the map.</param>
        /// <remarks>
        /// This method removes the terrain layout from both the underlying map and the observable collection,
        /// ensuring the UI is automatically updated.
        /// </remarks>
        public void RemoveTerrainLayout(TerrainLayoutViewModel layoutViewModel)
        {
            _map.TerrainLayouts.Remove(layoutViewModel.Model);
            TerrainLayouts.Remove(layoutViewModel);
        }
    }
}