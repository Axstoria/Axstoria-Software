namespace Controler.Editor.ViewModels
{
    using Loxodon.Framework.Observables;
    using Domain;
    using Domain.Math;
    
    /// <summary>
    /// ViewModel for Transform data in the map editor, representing position, rotation, and scale of scene objects.
    /// </summary>
    public class TransformViewModel : ObservableObject
    {
        public ObservableProperty<Vector3> Position { get; } = new ObservableProperty<Vector3>(Vector3.zero);   
        public ObservableProperty<Quaternion> Rotation { get; } =  new ObservableProperty<Quaternion>(Quaternion.identity);
        public ObservableProperty<Vector3> Scale { get; } =  new ObservableProperty<Vector3>(Vector3.one);
        
        private readonly TransformModel _model;

        /// <summary>
        /// Initializes a new instance of the TransformViewModel class with the given TransformModel.
        /// </summary>
        /// <param name="model"></param>
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