using Loxodon.Framework.Observables;
using SceneEditor.Domain;

namespace SceneEditor.Presenter.ViewModels
{
    public class ObjectViewModel : SceneViewModel
    {
        private readonly SceneObject _object;
        public SceneObject Model => _object;

        public ObservableProperty<bool>   IsInteractable { get; } = new();
        public ObservableProperty<string> DisplayName    { get; } = new();
        public ObservableProperty<string> Category       { get; } = new();
        public ObservableProperty<bool>   IsImported     { get; } = new();

        public ObjectViewModel(SceneObject sceneObject) : base(sceneObject)
        {
            _object = sceneObject;

            IsInteractable.Value = _object.IsInteractable;
            DisplayName.Value    = _object.DisplayName;
            Category.Value       = _object.Category;
            IsImported.Value     = _object.IsImported;

            IsInteractable.ValueChanged += (_, __) => _object.IsInteractable = IsInteractable.Value;
            DisplayName.ValueChanged    += (_, __) => _object.DisplayName    = DisplayName.Value;
            Category.ValueChanged       += (_, __) => _object.Category       = Category.Value;
        }
    }
}
