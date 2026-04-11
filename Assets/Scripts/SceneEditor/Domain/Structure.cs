namespace Domain
{
    /// <summary>
    /// Represents a structure or environmental element within a scene.
    /// Inherits from SceneModel and adds destructibility properties.
    /// </summary>
    public class Structure : SceneModel
    {
        public bool IsDestructible { get; set; }
    }
}