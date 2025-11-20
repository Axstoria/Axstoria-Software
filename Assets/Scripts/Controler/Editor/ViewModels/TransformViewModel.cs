using Unity.Mathematics;
using UnityEngine;

namespace Controler.Editor.ViewModels
{
    using Loxodon.Framework.Observables;
    using Domain;
    public class TransformViewModel : ObservableObject
    {
        public ObservableProperty<Vector3> Position { get; } = new ObservableProperty<Vector3>(Vector3.zero);
        public ObservableProperty<Quaternion> Rotation { get; } =  new ObservableProperty<Quaternion>(Quaternion.identity);
        public ObservableProperty<Vector3> Scale { get; } =  new ObservableProperty<Vector3>(Vector3.one);
        
        private readonly TransformModel _model;

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