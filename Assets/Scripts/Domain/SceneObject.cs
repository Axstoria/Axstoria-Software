using System.Collections.Generic;

namespace Domain
{
    /// <summary>
    /// Represents a non-token entity in the scene, such as furniture, props, or decorative items.
    /// </summary>
    public class SceneObject : SceneModel
    {
        public bool   IsInteractable  { get; set; }

        public string DisplayName     { get; set; }
        public string Category        { get; set; }
        public bool   IsImported      { get; set; }
        public string ImportPath      { get; set; }
    }
}
