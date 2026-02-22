using Domain.Math;

namespace Domain
{
    /// <summary>
    /// Represents the transformation properties (position, rotation, scale) of an object in 3D space.
    /// </summary>
    /// <remarks>
    /// This model encapsulates spatial data independent of Unity's GameObject transform system,
    /// allowing it to be used in data serialization and MVVM contexts.
    /// </remarks>
    public class TransformModel
    {
        /// <summary>
        /// Gets or sets the world position of the object.
        /// </summary>
        public Vector3 Position { get; set; }
        
        /// <summary>
        /// Gets or sets the rotation of the object in quaternion format.
        /// </summary>
        /// <remarks>
        /// Defaults to identity (no rotation).
        /// </remarks>
        public Quaternion Rotation { get; set; } = Quaternion.identity;
        
        /// <summary>
        /// Gets or sets the scale of the object.
        /// </summary>
        /// <remarks>
        /// Defaults to (1, 1, 1) indicating no scale applied.
        /// </remarks>
        public Vector3 Scale { get; set; } = Vector3.one;
    }
    
    /// <summary>
    /// Abstract base class for all scene objects that exist within a map.
    /// Provides common properties for entities like tokens, structures, and scene objects.
    /// </summary>
    /// <remarks>
    /// This class should not be instantiated directly. Instead, use derived classes such as
    /// Token, Structure, SceneObject, or TerrainLayout.
    /// </remarks>
    public abstract class SceneModel
    {
        /// <summary>
        /// Gets or sets the unique identifier for this scene object.
        /// </summary>
        public string Id { get; set; }
        
        /// <summary>
        /// Gets or sets the asset path or model identifier for this object.
        /// </summary>
        /// <remarks>
        /// This typically points to a prefab or model resource in the project.
        /// </remarks>
        public string ModelPath { get; set; }
        
        /// <summary>
        /// Gets or sets the transformation data (position, rotation, scale) for this object.
        /// </summary>
        public TransformModel Transform { get; set; } = new TransformModel();
        
        /// <summary>
        /// Gets or sets the preset or template identifier for this object.
        /// </summary>
        /// <remarks>
        /// This may reference predefined configurations or visual presets.
        /// </remarks>
        public string PresetId { get; set; }
        
        /// <summary>
        /// Gets the rendering or interaction layer this object belongs to.
        /// </summary>
        /// <remarks>
        /// Default layer is "base". Derived classes can override this to specify custom layers.
        /// </remarks>
        public virtual string Layer => "base";
    }
}