using Unity.Mathematics;
using UnityEngine;

namespace Domain
{

    public class TransformModel
    {
        public Vector3 Position { get; set; }
        public quaternion Rotation { get; set; } = Quaternion.identity;
        public Vector3 Scale { get; set; } = Vector3.one;
    }
    public abstract class SceneModel
    {
        public string Id { get; set; }
        public string ModelPath { get; set; }
        
        public TransformModel Transform { get; set; } = new TransformModel();
        
        public string PresetId { get; set; }
        public virtual string Layer => "base";
    }
}