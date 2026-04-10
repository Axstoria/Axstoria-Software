using System;
using Domain.Math;

namespace Domain
{
    public class TransformModel
    {
        public Vector3    Position { get; set; }
        public Quaternion Rotation { get; set; }
        public Vector3    Scale    { get; set; }
    }

    public abstract class SceneModel
    {
        public string         Id        { get; set; }
        public string         ModelPath { get; set; }
        public string         PresetId  { get; set; }
        public virtual string Layer     { get; set; }

        private TransformModel _transform;
        public TransformModel Transform
        {
            get => _transform;
            set { _transform = value; OnTransformChanged?.Invoke(this, EventArgs.Empty); }
        }

        public event EventHandler OnTransformChanged;
    }
}
