using Loxodon.Framework.Observables;
using SceneEditor.Domain;
using UnityEngine;

namespace MapEditor.Presenter.ViewModels
{
    public class TransformViewModel : ObservableObject
    {
        public ObservableProperty<Vector3>    Position { get; } = new ObservableProperty<Vector3>(Vector3.zero);
        public ObservableProperty<Quaternion> Rotation { get; } = new ObservableProperty<Quaternion>(Quaternion.identity);
        public ObservableProperty<Vector3>    Scale    { get; } = new ObservableProperty<Vector3>(Vector3.one);

        private readonly TransformModel _model;

        public TransformViewModel(TransformModel model)
        {
            _model = model;
            Position.Value = _model.Position;
            Rotation.Value = _model.Rotation;
            Scale.Value    = _model.Scale;

            Position.ValueChanged += (_, __) => _model.Position = Position.Value;
            Rotation.ValueChanged += (_, __) => _model.Rotation = Rotation.Value;
            Scale.ValueChanged    += (_, __) => _model.Scale    = Scale.Value;
        }
    }
}
