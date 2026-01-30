namespace Controler.Editor.ViewModels
{
    using Loxodon.Framework.Observables;
    using Domain;
    using Controler.Math;
    
    /// <summary>
    /// ViewModel for managing and exposing transformation properties (position, rotation, scale) to the UI.
    /// Provides observable properties that automatically synchronize with the underlying TransformModel.
    /// </summary>
    /// <remarks>
    /// This view model uses the MVVM pattern with Loxodon Framework's observable properties to ensure
    /// that any changes in the UI are immediately reflected in the model, and vice versa.
    /// This enables two-way data binding for transform-related UI elements.
    /// </remarks>
    public class TransformViewModel : ObservableObject
    {
        /// <summary>
        /// Gets the observable property for the object's position in 3D space.
        /// </summary>
        /// <remarks>
        /// Changes to this property are automatically synchronized back to the underlying TransformModel.
        /// Initialized to Vector3.zero.
        /// </remarks>
        public ObservableProperty<Vector3> Position { get; } = new ObservableProperty<Vector3>(Vector3.zero);
        
        /// <summary>
        /// Gets the observable property for the object's rotation.
        /// </summary>
        /// <remarks>
        /// Changes to this property are automatically synchronized back to the underlying TransformModel.
        /// Initialized to Quaternion.identity (no rotation).
        /// </remarks>
        public ObservableProperty<Quaternion> Rotation { get; } =  new ObservableProperty<Quaternion>(Quaternion.identity);
        
        /// <summary>
        /// Gets the observable property for the object's scale.
        /// </summary>
        /// <remarks>
        /// Changes to this property are automatically synchronized back to the underlying TransformModel.
        /// Initialized to Vector3.one (default scale).
        /// </remarks>
        public ObservableProperty<Vector3> Scale { get; } =  new ObservableProperty<Vector3>(Vector3.one);
        
        /// <summary>
        /// The underlying TransformModel that this view model represents.
        /// </summary>
        private readonly TransformModel _model;

        /// <summary>
        /// Initializes a new instance of the TransformViewModel class.
        /// </summary>
        /// <param name="model">The TransformModel to be wrapped by this view model.</param>
        /// <remarks>
        /// This constructor initializes all observable properties with values from the model
        /// and sets up two-way data binding between the view model and the model.
        /// </remarks>
        public TransformViewModel(TransformModel model)
        {
            _model = model;
            Position.Value = _model.Position;
            Rotation.Value = _model.Rotation;
            Scale.Value = _model.Scale;
            
            Position.ValueChanged += (sender, args) =>
            {
                _model.Position = Position.Value;
            };
            Rotation.ValueChanged += (sender, args) =>
            {
                _model.Rotation = Rotation.Value;
            };

            Scale.ValueChanged += (sender, args) =>
            {
                _model.Scale = Scale.Value;
            };
        }
    }
}