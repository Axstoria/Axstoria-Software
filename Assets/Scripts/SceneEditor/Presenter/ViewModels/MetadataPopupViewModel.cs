using Loxodon.Framework.Observables;
using SceneEditor.Domain;

namespace SceneEditor.Presenter.ViewModels
{
    public class MetadataPopupViewModel : SceneViewModel
    {
        public ObservableProperty<string> EntryType { get; } = new();
        public ObservableProperty<string> EntryValue { get; } = new();
        public ObservableProperty<string> IsVisible { get; } = new();

        public MetadataPopupViewModel(SceneObject sceneObject) : base(sceneObject)
        {
        }
    }
}
