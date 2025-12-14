using Domain;
using Loxodon.Framework.Observables;

namespace Controler.Editor.ViewModels
{
    /// <summary>
    /// View model for managing interactive scene objects within the map editor.
    /// Extends SceneViewModel to add properties specific to interactive objects such as doors and chests.
    /// </summary>
    /// <remarks>
    /// This view model handles the presentation and binding of scene object data to the UI,
    /// allowing editors to configure whether objects are interactable and other properties.
    /// </remarks>
    public class ObjectViewModel : SceneViewModel
    {
        /// <summary>
        /// Gets the observable property indicating whether this object is interactable.
        /// </summary>
        /// <remarks>
        /// Changes to this property are automatically synchronized back to the underlying SceneObject model.
        /// Initialized to false (not interactable by default).
        /// </remarks>
        public readonly ObservableProperty<bool> IsInteractable =  new ObservableProperty<bool>(false);
        
        /// <summary>
        /// The underlying SceneObject model that this view model represents.
        /// </summary>
        private readonly SceneObject _object;
        
        /// <summary>
        /// Gets the underlying SceneObject model.
        /// </summary>
        public SceneObject Model => _object;

        /// <summary>
        /// Initializes a new instance of the ObjectViewModel class.
        /// </summary>
        /// <param name="sceneObject">The SceneObject model to be wrapped by this view model.</param>
        /// <remarks>
        /// This constructor initializes the IsInteractable observable property with the object's current state
        /// and sets up two-way data binding between the view model and the model.
        /// </remarks>
        public ObjectViewModel(SceneObject sceneObject) : base(sceneObject)
        {
            _object = sceneObject;
            IsInteractable.Value = _object.IsInteractable;
            
            IsInteractable.ValueChanged += (sender, args) =>
            {
                _object.IsInteractable = IsInteractable.Value;
            };
        }
    }
}