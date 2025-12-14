namespace Domain
{
    /// <summary>
    /// Represents a structure or environmental element within a scene.
    /// Inherits from SceneModel and adds destructibility properties.
    /// </summary>
    /// <remarks>
    /// Structures include buildings, walls, barricades, or other architectural elements that can be
    /// either destructible (can be broken) or permanent features of the environment.
    /// </remarks>
    public class Structure : SceneModel
    {
        /// <summary>
        /// Gets or sets a value indicating whether this structure can be destroyed or damaged.
        /// </summary>
        /// <remarks>
        /// If true, the structure can take damage and potentially be removed from the map.
        /// If false, the structure is permanent and indestructible.
        /// </remarks>
        public bool IsDestructible { get; set; }
    }
}