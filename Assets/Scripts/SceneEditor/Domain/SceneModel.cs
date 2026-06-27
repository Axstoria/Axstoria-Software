using System;
using System.Collections.Generic;
using Shared.Domain;
using UnityEngine;

namespace SceneEditor.Domain
{
    public class TransformModel
    {
        public Vector3    Position { get; set; }
        public Quaternion Rotation { get; set; }
        public Vector3    Scale    { get; set; }
    }

    public abstract class SceneModel : IHasTags
    {
        public string          Id        { get; set; }
        public string          ModelPath { get; set; }
        public HashSet<string> Tags      { get; set; } = new();

        private TransformModel _transform = new TransformModel { Scale = UnityEngine.Vector3.one, Rotation = UnityEngine.Quaternion.identity };
        public TransformModel Transform
        {
            get => _transform;
            set { _transform = value; OnTransformChanged?.Invoke(this, EventArgs.Empty); }
        }

        public event EventHandler OnTransformChanged;
    }
}
