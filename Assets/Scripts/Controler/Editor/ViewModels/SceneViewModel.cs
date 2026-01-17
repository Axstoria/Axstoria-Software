using Loxodon.Framework.Observables;
using Domain;

namespace Controler.Editor.ViewModels
{
    /// <summary>
    /// Base view model for all scene objects, providing common properties and transformation management.
    /// Serves as the parent class for specific scene element view models such as tokens, structures, and objects.
    /// </summary>
    /// <remarks>
    /// This abstract-like view model encapsulates the common functionality for any scene object,
    /// including transform management and read-only model properties.
    /// Derived classes can extend this to provide additional properties specific to their entity type.
    /// </remarks>
    public class SceneViewModel : ObservableObject
    {
        /// <summary>
        /// The underlying SceneModel that this view model represents.
        /// </summary>
        private readonly SceneModel _model;
        
        /// <summary>
        /// Gets the view model for the object's transformation properties.
        /// </summary>
        /// <remarks>
        /// This provides access to position, rotation, and scale properties with automatic synchronization.
        /// </remarks>
        public TransformViewModel Transform { get; }

        /// <summary>
        /// Gets the unique identifier of the scene object.
        /// </summary>
        public string Id => _model.Id;
        
        /// <summary>
        /// Gets the model path or asset identifier for this scene object.
        /// </summary>
        public string ModelPath => _model.ModelPath;

        /// <summary>
        /// Initializes a new instance of the SceneViewModel class.
        /// </summary>
        /// <param name="model">The SceneModel to be wrapped by this view model.</param>
        /// <remarks>
        /// This constructor initializes the TransformViewModel with the model's transform data.
        /// </remarks>
        public SceneViewModel(SceneModel model)
        {
            _model = model;

            Transform = new TransformViewModel(model.Transform);
        }
    }
}