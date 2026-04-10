using System;
using Domain.Math;

namespace Domain
{
    /// <summary>
    /// Represents the position, rotation, and scale of an entity in the scene.
    /// </summary>
    public class TransformModel
    {
        public Vector3    Position { get; set; }
        public Quaternion Rotation { get; set; }
        public Vector3    Scale    { get; set; }
    }

    /// <summary>
    /// Base class for all entities that can be placed in the scene, including tokens, structures, and objects.
    /// </summary>
    public abstract class SceneModel
    {
        public string         Id        { get; set; }
        public string         ModelPath { get; set; }
        private TransformModel _transform;
        public TransformModel Transform
        {
            get => _transform;
            set { _transform = value; OnTransformChanged?.Invoke(this, EventArgs.Empty); }
        }

        public event EventHandler OnTransformChanged;
    }
}
