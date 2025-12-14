using Domain;
using Loxodon.Framework.Observables;

namespace Controler.Editor.ViewModels
{
    /// <summary>
    /// View model for managing terrain and height-map elements within the map editor.
    /// Extends SceneViewModel to handle terrain layout properties such as width and height.
    /// </summary>
    /// <remarks>
    /// This view model handles the presentation and binding of terrain layout data to the UI,
    /// allowing editors to configure and manage the geographical features of the map.
    /// </remarks>
    public class TerrainLayoutViewModel : SceneViewModel
    {
        /// <summary>
        /// The underlying TerrainLayout model that this view model represents.
        /// </summary>
        private readonly TerrainLayout _terrainLayout;
        
        /// <summary>
        /// Gets the underlying TerrainLayout model.
        /// </summary>
        public TerrainLayout Model => _terrainLayout;

        /// <summary>
        /// Initializes a new instance of the TerrainLayoutViewModel class.
        /// </summary>
        /// <param name="terrainLayout">The TerrainLayout model to be wrapped by this view model.</param>
        /// <remarks>
        /// This constructor initializes the view model with the terrain layout's data.
        /// Additional observable properties for Width and Height can be added as needed
        /// if direct UI binding to these properties is required.
        /// </remarks>
        public TerrainLayoutViewModel(TerrainLayout terrainLayout) : base(terrainLayout)
        {
            _terrainLayout = terrainLayout;
        }
    }
}