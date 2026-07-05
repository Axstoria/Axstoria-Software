namespace SceneEditor.Domain
{
    public class SceneObject : SceneModel
    {
        public bool   IsInteractable { get; set; }
        public string DisplayName    { get; set; }
        public string Category       { get; set; }
        public bool   IsImported     { get; set; }
        public string ImportPath     { get; set; }
    }
}
