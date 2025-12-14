namespace Domain
{
    /// <summary>
    /// Represents an interactive object within a scene.
    /// Inherits from SceneModel and adds interactability properties.
    /// </summary>
    /// <remarks>
    /// Scene objects can be doors, chests, levers, or any other environmental element that players can interact with.
    /// </remarks>
    public class SceneObject : SceneModel
    {
        /// <summary>
        /// Gets or sets a value indicating whether this object can be interacted with by the player.
        /// </summary>
        public bool IsInteractable { get; set; }
    }
}